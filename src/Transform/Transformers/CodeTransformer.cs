using System;
using System.Collections.Generic;
using System.Linq;

namespace CSharpE.Transform.Transformers
{
    internal class CodeTransformer<TInput> : Transformer<TInput>
    {
        private readonly Action<TInput> codeAction;
        private List<Transformer<TInput>> transformers;

        public static CodeTransformer<TInput> Create(Action<TInput> codeAction) =>
            new CodeTransformer<TInput>(codeAction);

        public CodeTransformer(Action<TInput> codeAction) => this.codeAction = codeAction;

        public override bool InputChanged(Diff<TInput> diff) => transformers?.Any(t => t.InputChanged(diff)) ?? true;

        public override void Transform(TransformProject project, Diff<TInput> diff)
        {
            // leaf code transformers can't be rerun on their own, so if any one of them needs to be rerun,
            // we have to rerun this whole transformer
            if (transformers?.Where(t => t is ILeafCodeTransformer).Any(t => t.InputChanged(diff)) ?? true)
            {
                var input = diff.GetNew();

                var transformerBuilder = new TransformerBuilder<TInput>(project, input);

                var oldTransformerBuilder = project.TransformerBuilder;
                project.TransformerBuilder = transformerBuilder;

                codeAction(input);

                project.TransformerBuilder = oldTransformerBuilder;

                this.transformers = transformerBuilder.Transformers;
            }
            else
            {
                foreach (var transformer in transformers)
                {
                    transformer.Transform(project, diff);
                }
            }
        }
    }

    // TODO: probably don't need this interface
    internal interface ILeafCodeTransformer { }
}