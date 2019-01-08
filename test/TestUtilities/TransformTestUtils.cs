using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CSharpE.Syntax;
using CSharpE.Transform;
using static System.Text.RegularExpressions.RegexOptions;

namespace CSharpE.TestUtilities
{
    public static class TransformTestUtils
    {
        public static string ProcessSingleFile(
            string code, ITransformation transformation, params Type[] additionalReferencesRepresentatives) =>
            ProcessSingleFile(code, transformation, false, additionalReferencesRepresentatives);

        public static string ProcessSingleFile(
            string code, ITransformation transformation, bool designTime,
            params Type[] additionalReferencesRepresentatives)
        {
            var project = new Project(
                new[] {new SourceFile("source.cse", code)}, CreateReferences(additionalReferencesRepresentatives));

            transformation.Process(project, designTime);

            return project.SourceFiles.Single().GetText();
        }

        private static readonly Regex Optional = new Regex(@"\[\|(.*?)\|\]", Singleline);

        public static string IgnoreOptional(string input) => Optional.Replace(input, string.Empty);
        public static string IncludeOptional(string input) => Optional.Replace(input, "$1");

        public static IEnumerable<LibraryReference> CreateReferences(params Type[] representatives) =>
            representatives.Prepend(typeof(object)).Select(t => new AssemblyReference(t));
    }
}