using System.Collections.Generic;
using System.Linq;

namespace CSharpE.Transform.Transformers
{
    internal class SequentialTransformer : Transformer
    {
        private readonly List<Transformer> transformers;

        public SequentialTransformer(IEnumerable<Transformer> transformers) =>
            this.transformers = transformers.ToList();

        public override void Transform(ProjectDiff diff)
        {
            foreach (var transformer in transformers)
            {
                transformer.Transform(diff);
            }
        }
    }
}