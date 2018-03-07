using CSharpE.Syntax;

namespace CSharpE.Transform
{
    public interface ITransformation<in TProject> where TProject : Project
    {
        void Process(TProject project);
    }

    public interface ITransformation : ITransformation<Project> { }

    public interface ISmartTransformation : ITransformation<SmartProject> { }
}