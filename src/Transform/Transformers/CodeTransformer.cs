using System;
using System.Collections.Generic;
using System.Linq;
using CSharpE.Syntax;
using CSharpE.Syntax.Internals;
using CSharpE.Transform.Internals;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Roslyn = Microsoft.CodeAnalysis;

namespace CSharpE.Transform.Transformers
{
    internal class CodeTransformer<TInput, TOutput> : Transformer<TInput, TOutput>
    {
        private readonly Func<TInput, TOutput> codeAction;

        private List<Transformer> transformers;

        public static CodeTransformer<TInput, TOutput> Create(Func<TInput, TOutput> codeAction, bool limitedComparison = false)
        {
            if (typeof(SyntaxNode).IsAssignableFrom(typeof(TInput)))
                return (CodeTransformer<TInput, TOutput>) Activator.CreateInstance(
                    typeof(SyntaxNodeCodeTransfomer<,>).MakeGenericType(typeof(TInput), typeof(TOutput)), codeAction,
                    limitedComparison);

            return new CodeTransformer<TInput, TOutput>(codeAction);
        }

        protected CodeTransformer(Func<TInput, TOutput> codeAction) => this.codeAction = codeAction;

        public override TOutput Transform(TransformProject project, TInput input)
        {
            var transformerBuilder = new TransformerBuilder(project, transformers);

            var oldTransformerBuilder = project.TransformerBuilder;
            project.TransformerBuilder = transformerBuilder;

            project.Log(typeof(TInput).Name, LogInfo.GetName(input), "transform");

            var result = codeAction(input);

            project.TransformerBuilder = oldTransformerBuilder;

            transformers = transformerBuilder.Transformers;

            return result;
        }
    }

    internal sealed class SyntaxNodeCodeTransfomer<TInput, TOutput> : CodeTransformer<TInput, TOutput>
        where TInput : SyntaxNode, ISyntaxWrapper<Roslyn::SyntaxNode>
    {
        private readonly bool limited;
        
        private Roslyn::SyntaxNode beforeSyntax;

        private TOutput cachedOutput;
        private Action<TInput> syntaxChangesApplier;

        public SyntaxNodeCodeTransfomer(Func<TInput, TOutput> codeAction, bool limited)
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

        Func<TypeDefinition, Func<TypeDefinition, Action<TypeDefinition>>> CreateLimitedDiffer()
        {
            return input1 =>
            {
                // first record the count of members before transformation
                
                var oldMembersCount = input1.Members.Count;

                return input2 =>
                {
                    // then record Roslyn syntax for nodes added by limited transformation 
                    
                    var newMembers = new List<MemberDeclarationSyntax>();

                    for (int i = oldMembersCount; i < input2.Members.Count; i++)
                    {
                        newMembers.Add(input2.Members[i].GetWrapped());
                    }

                    // finally add recorded nodes to a node
                    
                    return input3 =>
                        input3.Members.AddRange(newMembers.Select(m => FromRoslyn.MemberDefinition(m, input3)));
                };
            };
        }

        Func<TInput, Func<TInput, Action<TInput>>> CreateDiffer()
        {
            if (limited)
                return (Func<TInput, Func<TInput, Action<TInput>>>) CreateLimitedDiffer();
            
            return _ => input =>
            {
                var afterSyntax = input.GetWrapped();
                return nextInput => nextInput.SetSyntax(afterSyntax);
            };
        }

        public override TOutput Transform(TransformProject project, TInput input)
        {
            var newBeforeSyntax = Limit(input.GetWrapped());

            if (beforeSyntax != null && beforeSyntax.IsEquivalentTo(newBeforeSyntax))
            {
                project.Log(typeof(TInput).Name, LogInfo.GetName(input), "cached");

                syntaxChangesApplier.Invoke(input);
            }
            else
            {
                var differ = CreateDiffer().Invoke(input);

                cachedOutput = base.Transform(project, input);
                
                beforeSyntax = newBeforeSyntax;

                syntaxChangesApplier = differ(input);
            }

            return Cloner.DeepClone(cachedOutput);
        }
    }
}