using System;
using System.Collections.Generic;
using System.Linq;
using CSharpE.Syntax;
using CSharpE.Syntax.Internals;
using CSharpE.Transform.Internals;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Roslyn = Microsoft.CodeAnalysis;
using SyntaxNode = CSharpE.Syntax.SyntaxNode;

namespace CSharpE.Transform.Transformers
{
    internal class CodeTransformer<TInput, TOutput>
    {
        private readonly Func<TInput, TOutput> codeAction;

        private List<CollectionTransformer> transformers;

        public static CodeTransformer<TInput, TOutput> Create(Func<TInput, TOutput> codeAction, bool limitedComparison = false)
        {
            if (typeof(SyntaxNode).IsAssignableFrom(typeof(TInput)))
                return (CodeTransformer<TInput, TOutput>)Activator.CreateInstance(
                    typeof(SyntaxNodeCodeTransformer<,>).MakeGenericType(typeof(TInput), typeof(TOutput)),
                    codeAction, limitedComparison);

            return new CodeTransformer<TInput, TOutput>(codeAction);
        }

        protected CodeTransformer(Func<TInput, TOutput> codeAction) => this.codeAction = codeAction;

        private protected virtual string GetTargetKind(TInput input) => input.GetType().Name; 

        public virtual TOutput Transform(TransformProject project, TInput input)
        {
            var transformerCollector = new TransformerCollector(project, transformers);

            var oldTransformerCollector = project.TransformerCollector;
            project.TransformerCollector = transformerCollector;

            project.Log(GetTargetKind(input), LogInfo.GetName(input), "transform");

            var result = codeAction(input);

            project.TransformerCollector = oldTransformerCollector;

            transformers = transformerCollector.Transformers;

            return GeneralHandler.DeepClone(result);
        }
    }

    internal sealed class SyntaxNodeCodeTransformer<TInput, TOutput> : CodeTransformer<TInput, TOutput>
        where TInput : SyntaxNode, ISyntaxWrapper<Roslyn::SyntaxNode>
    {
        private readonly bool limited;
        
        private Roslyn::SyntaxNode beforeSyntax;

        private TOutput cachedOutput;
        private Action<TInput> syntaxChangesApplier;

        public SyntaxNodeCodeTransformer(Func<TInput, TOutput> codeAction, bool limited)
            : base(codeAction)
            => this.limited = limited;

        private Roslyn::SyntaxNode Limit(Roslyn::SyntaxNode node)
        {
            if (!limited)
                return node;

            switch (node)
            {
                case ClassDeclarationSyntax classDeclaration: return classDeclaration.WithMembers(default);
                case StructDeclarationSyntax structDeclaration: return structDeclaration.WithMembers(default);
                case InterfaceDeclarationSyntax interfaceDeclaration: return interfaceDeclaration.WithMembers(default);
                default: throw new InvalidOperationException();
            }
        }

        private static Func<TypeDefinition, Func<Action<TypeDefinition>>> CreateLimitedDiffer()
        {
            return input =>
            {
                // first record the count of members before transformation
                
                var oldMembersCount = input.Members.Count;

                return () =>
                {
                    // then record Roslyn syntax for nodes added by limited transformation 

                    var newMembers = input.Members.Skip(oldMembersCount).Select(m => m.GetWrapped()).ToList();

                    // finally add recorded nodes to a node
                    
                    return nextInput =>
                        nextInput.Members.AddRange(newMembers.Select(m => FromRoslyn.MemberDefinition(m, nextInput)));
                };
            };
        }

        private Func<TInput, Func<Action<TInput>>> CreateDiffer()
        {
            if (limited)
                return AddNamespacesToDiffer((Func<TInput, Func<Action<TInput>>>)CreateLimitedDiffer());

            return AddNamespacesToDiffer(input => () =>
            {
                var afterSyntax = input.GetWrapped();
                return nextInput =>
                    nextInput.SetSyntax(TranslateAnnotations(beforeSyntax, nextInput.GetWrapped(), afterSyntax));
            });
        }

        private static Roslyn::SyntaxNode TranslateAnnotations(
            Roslyn::SyntaxNode beforeSyntax, Roslyn::SyntaxNode newBeforeSyntax, Roslyn::SyntaxNode afterSyntax)
        {
            var annotationMap = new Dictionary<SyntaxAnnotation, SyntaxAnnotation>();

            var nodePairs = beforeSyntax.DescendantNodesAndSelf().Zip(newBeforeSyntax.DescendantNodesAndSelf());

            foreach (var (beforeNode, newBeforeNode) in nodePairs)
            {
                var newAnnotation = Annotation.Get(newBeforeNode);
                if (newAnnotation == null)
                    continue;

                var oldAnnotation = Annotation.Get(beforeNode);
                if (oldAnnotation == null)
                    continue;

                if (!annotationMap.ContainsKey(oldAnnotation))
                    annotationMap.Add(oldAnnotation, newAnnotation);
            }

            return afterSyntax.ReplaceNodes(
                afterSyntax.DescendantNodesAndSelf(), (_, n) =>
                {
                    var oldAnnotation = Annotation.Get(n);

                    if (oldAnnotation == null)
                        return n;

                    if (!annotationMap.TryGetValue(oldAnnotation, out var newAnnotation))
                        return n;

                    return n.WithoutAnnotations(oldAnnotation).WithAdditionalAnnotations(newAnnotation);
                });
        }

        private static Func<TInput, Func<Action<TInput>>> AddNamespacesToDiffer(Func<TInput, Func<Action<TInput>>> differ)
        {
            return input =>
            {
                var recorder = input.SourceFile.RecordUsingNamespaces();

                var differBefore = differ(input);

                return () =>
                {
                    var namespaces = recorder.StopAndGetResult();

                    var differAfter = differBefore();

                    return nextInput =>
                    {
                        differAfter(nextInput);

                        foreach (var ns in namespaces)
                        {
                            nextInput.SourceFile.EnsureUsingNamespace(ns);
                        }
                    };
                };
            };
        }

        private protected override string GetTargetKind(TInput input) =>
            limited ? "Segment" : base.GetTargetKind(input);

        public override TOutput Transform(TransformProject project, TInput input)
        {
            var newBeforeSyntax = Limit(input.GetWrapped());

            if (beforeSyntax != null && beforeSyntax.IsEquivalentTo(newBeforeSyntax))
            {
                project.Log(GetTargetKind(input), LogInfo.GetName(input), "cached");

                syntaxChangesApplier.Invoke(input);
            }
            else
            {
                var differ = CreateDiffer().Invoke(input);

                cachedOutput = base.Transform(project, input);
                
                beforeSyntax = newBeforeSyntax;

                syntaxChangesApplier = differ();
            }

            return GeneralHandler.DeepClone(cachedOutput);
        }
    }
}
