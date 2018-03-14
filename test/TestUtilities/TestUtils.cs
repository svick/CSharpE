using System;
using System.Linq;
using System.Text.RegularExpressions;
using CSharpE.Transform;
using static System.Text.RegularExpressions.RegexOptions;

namespace CSharpE.TestUtilities
{
    public static class TestUtils
    {
        public static string ProcessSingleFile(string code, ITransformation transformation, params Type[] additionalReferencesRepresentatives)
        {
            var project = new Syntax.Project(new[] { new Syntax.SourceFile("source.cse", code) }, additionalReferencesRepresentatives);
            
            transformation.Process(project);

            return project.SourceFiles.Single().GetText();
        }

        private static readonly Regex Optional = new Regex(@"\[\|(.*?)\|\]", Compiled | Singleline);

        public static string IgnoreOptional(string input) => Optional.Replace(input, string.Empty);
        public static string Includeptional(string input) => Optional.Replace(input, "$1");
    }
}