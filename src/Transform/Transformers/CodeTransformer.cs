using System;
using System.Collections.Generic;
using CSharpE.Syntax;
using CSharpE.Syntax.Internals;
using Roslyn = Microsoft.CodeAnalysis;

namespace CSharpE.Transform.Transformers
{
    internal class CodeTransformer<TInput> : Transformer<TInput>
    {
        private readonly Action<TInput> codeAction;

        private List<Transformer> transformers;

        public static CodeTransformer<TInput> Create(Action<TInput> codeAction)
        {
            if (typeof(SyntaxNode).IsAssignableFrom(typeof(TInput)))
                return (CodeTransformer<TInput>)Activator.CreateInstance(
                    typeof(SyntaxNodeCodeTransfomer<>).MakeGenericType(typeof(TInput)), codeAction);

            return new CodeTransformer<TInput>(codeAction);
        }

        protected CodeTransformer(Action<TInput> codeAction) => this.codeAction = codeAction;

        public override void Transform(TransformProject project, TInput input)
        {
            var transformerBuilder = new TransformerBuilder(project, transformers);

            var oldTransformerBuilder = project.TransformerBuilder;
            project.TransformerBuilder = transformerBuilder;

            codeAction(input);

            project.TransformerBuilder = oldTransformerBuilder;

            transformers = transformerBuilder.Transformers;
        }
    }

    internal sealed class SyntaxNodeCodeTransfomer<TInput> : CodeTransformer<TInput>
        where TInput : SyntaxNode, ISyntaxWrapper<Roslyn::SyntaxNode>
    {
        private Roslyn::SyntaxNode beforeSyntax;
        private Roslyn::SyntaxNode afterSyntax;

        public SyntaxNodeCodeTransfomer(Action<TInput> codeAction) : base(codeAction) { }

        public override void Transform(TransformProject project, TInput input)
        {
            var newBeforeSyntax = input.GetWrapped();

            if (beforeSyntax != null && beforeSyntax.IsEquivalentTo(newBeforeSyntax))
            {
                input.SetSyntax(afterSyntax);
            }
            else
            {
                base.Transform(project, input);

                beforeSyntax = newBeforeSyntax;
                afterSyntax = input.GetWrapped();
            }
        }
    }
}