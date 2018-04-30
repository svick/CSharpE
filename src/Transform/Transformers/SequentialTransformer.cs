using System.Collections.Generic;
using System.Linq;

namespace CSharpE.Transform.Transformers
{
    internal class SequentialTransformer<TInput> : Transformer<TInput>
    {
        private readonly List<Transformer<TInput>> transformers;

        public SequentialTransformer(IEnumerable<Transformer<TInput>> transformers) =>
            this.transformers = transformers.ToList();

        public override bool InputChanged(Diff<TInput> diff) => transformers.Any(t => t.InputChanged(diff));

        public override void Transform(TransformProject project, Diff<TInput> diff)
        {
            foreach (var transformer in transformers)
            {
                transformer.Transform(project, diff);
            }
        }
    }
}