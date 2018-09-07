namespace CSharpE.Transform
{
    public abstract class SimpleTransformation : Transformation
    {
        public sealed override void Process(Syntax.Project project, bool designTime) => Process(project);

        protected abstract void Process(Syntax.Project project);
    }
}