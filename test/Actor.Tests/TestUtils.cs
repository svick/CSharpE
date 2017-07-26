using System.Linq;
using CSharpE.Syntax;
using CSharpE.Transform;

namespace Actor.Tests
{
    static class TestUtils
    {
        public static string ProcessSingleFile(string code, ITransformation transformation)
        {
            var project = new Project(new SourceFile("source.cse", code));
            
            transformation.Process(project);

            return project.SourceFiles.Single().GetText();
        }
    }
}