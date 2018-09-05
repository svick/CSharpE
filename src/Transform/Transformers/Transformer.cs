using CSharpE.Transform.Internals;

namespace CSharpE.Transform.Transformers
{
    internal abstract class Transformer { }

    internal abstract class Transformer<TInput, TOutput> : Transformer
    {
        public abstract TOutput Transform(TransformProject project, TInput input);
    }

    internal class TransformationTransformer
    {
        private readonly ITransformation transformation;

        private Transformer<TransformProject, Unit> innerTransformer;

        public TransformationTransformer(ITransformation transformation) => this.transformation = transformation;

        public void Transform(TransformProject project)
        {
            if (innerTransformer == null)
            {
                innerTransformer = project.RunTransformation(transformation);
            }
            else
            {
                innerTransformer.Transform(project, project);
            }
        }
    }
}