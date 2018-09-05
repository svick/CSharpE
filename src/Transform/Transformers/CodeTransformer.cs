using System;
using System.Collections.Generic;
using CSharpE.Syntax;
using CSharpE.Syntax.Internals;
using CSharpE.Transform.Internals;
using Roslyn = Microsoft.CodeAnalysis;

namespace CSharpE.Transform.Transformers
{
    internal class CodeTransformer<TInput, TOutput> : Transformer<TInput, TOutput>
    {
        private readonly Func<TInput, TOutput> codeAction;

        private List<Transformer> transformers;

        public static CodeTransformer<TInput, TOutput> Create(Func<TInput, TOutput> codeAction)
        {
            if (typeof(SyntaxNode).IsAssignableFrom(typeof(TInput)))
                return (CodeTransformer<TInput, TOutput>) Activator.CreateInstance(
                    typeof(SyntaxNodeCodeTransfomer<,>).MakeGenericType(typeof(TInput), typeof(TOutput)), codeAction);

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
        private Roslyn::SyntaxNode beforeSyntax;
        private Roslyn::SyntaxNode afterSyntax;

        private TOutput cachedOutput;

        public SyntaxNodeCodeTransfomer(Func<TInput, TOutput> codeAction) : base(codeAction) { }

        public override TOutput Transform(TransformProject project, TInput input)
        {
            var newBeforeSyntax = input.GetWrapped();

            if (beforeSyntax != null && beforeSyntax.IsEquivalentTo(newBeforeSyntax))
            {
                project.Log(typeof(TInput).Name, LogInfo.GetName(input), "cached");

                input.SetSyntax(afterSyntax);
            }
            else
            {
                cachedOutput = base.Transform(project, input);

                beforeSyntax = newBeforeSyntax;
                afterSyntax = input.GetWrapped();
            }

            return Cloner.DeepClone(cachedOutput);
        }
    }
}