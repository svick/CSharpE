using System.Linq;
using CSharpE.Syntax;

namespace CSharpE.TestUtilities
{
    public static class SyntaxTestUtils
    {
        public static SourceFile CreateSourceFile(string code)
        {
            var sourceFile = new SourceFile("source.cse", code);

            var project = new Project(sourceFile);

            return project.SourceFiles.Single();
        }
    }
}