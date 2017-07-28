using System;
using System.Linq;
using CSharpE.Syntax;
using CSharpE.Transform;

namespace TestUtilities
{
    public static class TestUtils
    {
        public static string ProcessSingleFile(string code, ITransformation transformation, params Type[] additionalReferencesRepresentatives)
        {
            var project = new Project(new[] { new SourceFile("source.cse", code) }, additionalReferencesRepresentatives);
            
            transformation.Process(project);

            return project.SourceFiles.Single().GetText();
        }
    }
}