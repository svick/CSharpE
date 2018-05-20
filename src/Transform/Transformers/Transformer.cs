namespace CSharpE.Transform.Transformers
{
    internal abstract class Transformer { }

    internal abstract class Transformer<TInput> : Transformer
    {
        public abstract void Transform(TransformProject project, TInput input);
    }

    internal class TransformationTransformer
    {
        private readonly ITransformation transformation;

        private Transformer<TransformProject> innerTransformer;

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