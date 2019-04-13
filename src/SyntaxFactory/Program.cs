using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CSharpE.Syntax;
using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.MSBuild;
using static CSharpE.Syntax.MemberModifiers;

namespace CSharpE.SyntaxFactory
{
    static class Program
    {
        static async Task Main()
        {
            MSBuildLocator.RegisterDefaults();

            var solution = await MSBuildWorkspace.Create().OpenSolutionAsync("../../CSharpE.sln");

            var roslynProject = solution.Projects.Single(p => p.Name == "Syntax(net46)");

            var project = new Project((CSharpCompilation)await roslynProject.GetCompilationAsync());

            var syntaxFactory = new ClassDefinition(Public | Static | Partial, "SyntaxFactory");

            foreach (var classDefinition in project.GetClasses())
            {
                if (!classDefinition.IsPublic || classDefinition.IsAbstract)
                    continue;

                if (!classDefinition.GetSymbol().HasBaseClass<SyntaxNode>())
                    continue;

                var typeReference = classDefinition.GetReference();

                foreach (var constructor in classDefinition.Constructors)
                {
                    if (constructor.IsInternal)
                        continue;

                    string methodName = typeReference.Name.TrimEnd("Expression").TrimEnd("Statement").TrimEnd("Reference");

                    // TODO: fix params
                    // TODO: somehow remove special cases
                    syntaxFactory.AddMethod(
                        Public | Static, typeReference, methodName, constructor.Parameters,
                        new ReturnStatement(
                            new NewExpression(
                                typeReference,
                                constructor.Parameters.Select(p => new Argument(new IdentifierExpression(p.Name))))));
                }
            }

            var sourceFile = new SourceFile("SyntaxFactory.g.cs")
            {
                Members = { new NamespaceDefinition("CSharpE.Syntax") { Members = { syntaxFactory } } }
            };

            File.WriteAllText(sourceFile.Path, sourceFile.ToString());
        }

        static string TrimEnd(this string input, string toTrim) =>
            input.EndsWith(toTrim) ? input.Remove(input.LastIndexOf(toTrim, StringComparison.Ordinal)) : input;
    }
}
