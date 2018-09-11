namespace CSharpE.Transform
{
    public abstract class BuildTimeTransformation : Transformation
    {
        public sealed override void Process(Syntax.Project project, bool designTime)
        {
            if (designTime)
                return;
            
            Process(project);
        }

        protected abstract void Process(Syntax.Project project);
    }
}