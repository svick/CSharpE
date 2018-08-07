using System;
using System.Collections.Generic;
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
            var project = new AdhocWorkspace().AddProject("Project", LanguageNames.CSharp)
                .AddMetadataReference(
                    MetadataReference.CreateFromFile(typeof(object).Assembly.Location));
            var document =
                project.AddDocument("Entities.cs", EntityKinds.ToGenerateFromSource);
            var g = SyntaxGenerator.GetGenerator(document);

            var compilationUnit = (await document.GetSyntaxTreeAsync()).GetRoot();
            var model = await document.GetSemanticModelAsync();
            var compilation = model.Compilation;

            compilationUnit = compilationUnit.ReplaceNodes(
                compilationUnit.DescendantNodes().OfType<ClassDeclarationSyntax>(),
                (_, classDeclaration) =>
                {
                    SyntaxNode result = classDeclaration;

                    result = g.AddBaseType(result, g.TypeExpression(
                        compilation.GetTypeByMetadataName("System.IComparable`1")
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

                    var statements = new List<SyntaxNode>();

                    statements.Add(g.LocalDeclarationStatement(
                        compilation.GetSpecialType(SpecialType.System_Int32), "result"));

                    foreach (var field in fields)
                    {
                        var fieldIdentifier = g.IdentifierName(g.GetName(field));

                        statements.Add(g.AssignmentStatement(g.IdentifierName("result"),
                            g.InvocationExpression(
                                g.MemberAccessExpression(fieldIdentifier, "CompareTo"),
                                g.MemberAccessExpression(g.IdentifierName("other"),
                                    fieldIdentifier))));

                        statements.Add(g.IfStatement(
                            g.ValueNotEqualsExpression(g.IdentifierName("result"),
                                g.LiteralExpression(0)),
                            new[] { g.ReturnStatement(g.IdentifierName("result")) }));
                    }

                    statements.Add(g.ReturnStatement(g.LiteralExpression(0)));

                    result = g.AddMembers(result, g.MethodDeclaration("CompareTo",
                        new[]
                        {
                            g.ParameterDeclaration("other", g.TypeExpression(
                                model.GetDeclaredSymbol(classDeclaration)))
                        },
                        returnType: g.TypeExpression(
                            compilation.GetSpecialType(SpecialType.System_Int32)),
                        accessibility: Accessibility.Public, statements: statements));

                    return result;
                });

            document = document.WithSyntaxRoot(compilationUnit.NormalizeWhitespace());
            document = await Simplifier.ReduceAsync(document);

            Console.WriteLine(await document.GetSyntaxRootAsync());
        }
    }
}