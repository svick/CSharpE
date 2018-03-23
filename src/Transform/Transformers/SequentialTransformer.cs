using System.Collections.Generic;
using System.Linq;

namespace CSharpE.Transform.Transformers
{
    internal class SequentialTransformer<TDiff> : Transformer<TDiff>
    {
        private readonly List<Transformer<TDiff>> transformers;

        public SequentialTransformer(IEnumerable<Transformer<TDiff>> transformers) =>
            this.transformers = transformers.ToList();

        public override bool InputChanged(TDiff diff) => transformers.Any(t => t.InputChanged(diff));

        public override void Transform(TransformProject project, TDiff diff)
        {
            foreach (var transformer in transformers)
            {
                transformer.Transform(project, diff);
            }
        }
    }
}