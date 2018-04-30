using System.Collections.Generic;
using System.Linq;

namespace CSharpE.Transform.Transformers
{
    internal static class Transformer
    {
        public static Transformer<TransformProject> Create(IEnumerable<ITransformation> transformations) =>
            new SequentialTransformer<TransformProject>(transformations.Select(Create));

        private static Transformer<TransformProject> Create(ITransformation transformation) =>
            new TransformationTransformer(transformation);
    }

    internal abstract class Transformer<TInput>
    {
        public abstract bool InputChanged(Diff<TInput> diff);

        public abstract void Transform(TransformProject project, Diff<TInput> diff);
    }

    internal static class TransformerExtensions
    {
        public static void Transform(this Transformer<TransformProject> transformer, Diff<TransformProject> diff) =>
            transformer.Transform(diff.GetNew(), diff);
    }

    internal class TransformationTransformer : Transformer<TransformProject>
    {
        private readonly ITransformation transformation;

        private Transformer<TransformProject> innerTransformer;

        public TransformationTransformer(ITransformation transformation) => this.transformation = transformation;

        // TODO: proper implementation
        public override bool InputChanged(Diff<TransformProject> diff) => true;

        public override void Transform(TransformProject project, Diff<TransformProject> diff)
        {
            if (innerTransformer == null)
            {
                var transformProject = diff.GetNew();

                var transformer = transformProject.RunTransformation(transformation);

                // TODO: add caching to transformer to make it faster if nothing relevant changed?
                innerTransformer = transformer;
            }
            else
            {
                innerTransformer.Transform(project, diff);
            }
        }
    }
}