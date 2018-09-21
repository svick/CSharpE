using CSharpE.Syntax;

namespace CSharpE.Transform
{
    public abstract class SimpleTransformation : Transformation
    {
        public sealed override void Process(Project project, bool designTime) => Process(project);

        protected abstract void Process(Project project);
    }
}