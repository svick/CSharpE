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

                // requires manually written SyntaxFactory methods
                switch (classDefinition.Name) {
                    case nameof(IntLiteralExpression):
                    case nameof(CharLiteralExpression):
                    case nameof(StringLiteralExpression):
                    case nameof(BoolLiteralExpression):
                    case nameof(TryStatement):
                        continue;
                }

                var typeReference = classDefinition.GetReference();

                foreach (var constructor in classDefinition.Constructors)
                {
                    if (constructor.IsInternal)
                        continue;

                    string methodName = typeReference.Name.TrimEnd("Expression").TrimEnd("Statement").TrimEnd("Reference");

                    var constructorParameters = constructor.Parameters;

                    if (classDefinition.Name.EndsWith(nameof(MemberAccessExpression)) || classDefinition.Name == nameof(ElementAccessExpression))
                        constructorParameters[0].Modifiers |= ParameterModifiers.This;

                    syntaxFactory.AddMethod(
                        Public | Static, typeReference, methodName, constructorParameters,
                        new ReturnStatement(
                            new NewExpression(
                                typeReference,
                                constructorParameters.Select(p => new Argument(new IdentifierExpression(p.Name))))));
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
