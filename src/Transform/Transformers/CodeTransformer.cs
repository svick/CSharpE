﻿using System;
using System.Collections.Generic;

namespace CSharpE.Transform.Transformers
{
    internal sealed class CodeTransformer<TInput> : Transformer<TInput>
    {
        private readonly Action<TInput> codeAction;
        private List<Transformer> transformers;

        public static CodeTransformer<TInput> Create(Action<TInput> codeAction) =>
            new CodeTransformer<TInput>(codeAction);

        public CodeTransformer(Action<TInput> codeAction) => this.codeAction = codeAction;

        public override void Transform(TransformProject project, TInput input)
        {
            var transformerBuilder = new TransformerBuilder<TInput>(project, input, transformers);

            var oldTransformerBuilder = project.TransformerBuilder;
            project.TransformerBuilder = transformerBuilder;

            codeAction(input);

            project.TransformerBuilder = oldTransformerBuilder;

            transformers = transformerBuilder.Transformers;
        }
    }
}