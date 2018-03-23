using System.Collections.Generic;
using System.Linq;

namespace CSharpE.Transform.Transformers
{
    internal static class Transformer
    {
        public static Transformer<ProjectDiff> Create(IEnumerable<ITransformation> transformations) =>
            new SequentialTransformer<ProjectDiff>(transformations.Select(Create));

        private static Transformer<ProjectDiff> Create(ITransformation transformation) =>
            new TransformationTransformer(transformation);
    }

    internal abstract class Transformer<TDiff>
    {
        public abstract bool InputChanged(TDiff diff);

        public abstract void Transform(TransformProject project, TDiff diff);
    }

    internal static class TransformerExtensions
    {
        public static void Transform<TDiff>(
            this Transformer<TDiff> transformer, TDiff diff) where TDiff : Diff<TransformProject> =>
            transformer.Transform(diff.GetNew(), diff);
    }

    internal class TransformationTransformer : Transformer<ProjectDiff>
    {
        private readonly ITransformation transformation;

        private Transformer<ProjectDiff> innerTransformer;

        public TransformationTransformer(ITransformation transformation) => this.transformation = transformation;

        // TODO: proper implementation
        public override bool InputChanged(ProjectDiff diff) => true;

        public override void Transform(TransformProject project, ProjectDiff diff)
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