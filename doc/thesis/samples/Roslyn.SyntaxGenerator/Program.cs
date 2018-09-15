using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CSharpE.Samples.Core;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Simplification;

namespace CSharpE.Samples.RoslynSyntaxGenerator
{
    static class Program
    {
        static async Task Main()
        {
            var project = new AdhocWorkspace().AddProject("MyProject", LanguageNames.CSharp)
                .AddMetadataReference(
                    MetadataReference.CreateFromFile(typeof(object).Assembly.Location));
            var document =
                project.AddDocument("Entities.cs", EntityKinds.ToGenerateFromSource);
            var g = SyntaxGenerator.GetGenerator(document);

            var rootNode = await document.GetSyntaxRootAsync();
            var model = await document.GetSemanticModelAsync();
            var compilation = model.Compilation;

            rootNode = rootNode.ReplaceNodes(
                rootNode.DescendantNodes().OfType<ClassDeclarationSyntax>(),
                (_, classDeclaration) =>
                {
                    SyntaxNode result = classDeclaration;

                    result = g.AddBaseType(result, g.TypeExpression(
                        compilation.GetTypeByMetadataName("System.IEquatable`1")
                            .Construct(model.GetDeclaredSymbol(classDeclaration))));

                    var fields = result.ChildNodes().OfType<FieldDeclarationSyntax>();

                    result = result.RemoveNodes(fields, default);

                    foreach (var fieldDeclaration in fields)
                    {
                        var type = fieldDeclaration.Declaration.Type;
                        var propertyName = g.GetName(fieldDeclaration);
                        var fieldName = propertyName.ToLowerInvariant();

                        var field = g.WithName(fieldDeclaration, fieldName);

                        var fieldAccess = g.IdentifierName(fieldName);
                        var property = g.PropertyDeclaration(
                            propertyName, type, Accessibility.Public,
                            getAccessorStatements: new[]
                            {
                                g.ReturnStatement(fieldAccess)
                            },
                            setAccessorStatements: new[]
                            {
                                g.AssignmentStatement(
                                    fieldAccess, g.IdentifierName("value"))
                            });

                        result = g.AddMembers(result, field, property);
                    }

                    return result;
                });

            document = document.WithSyntaxRoot(rootNode.NormalizeWhitespace());
            document = await Simplifier.ReduceAsync(document);

            File.WriteAllText(
                "Entities.cs", (await document.GetSyntaxRootAsync()).ToString());
        }
    }
}