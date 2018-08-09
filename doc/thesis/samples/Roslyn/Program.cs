using System.IO;
using System.Linq;
using CSharpE.Samples.Core;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Microsoft.CodeAnalysis.CSharp.SyntaxKind;

namespace CSharpE.Samples.Roslyn
{
    static class Program
    {
        static void Main()
        {
            var compilationUnit = ParseCompilationUnit(EntityKinds.ToGenerateFromSource);

            compilationUnit = compilationUnit.ReplaceNodes(
                compilationUnit.DescendantNodes().OfType<ClassDeclarationSyntax>(),
                (_, classDeclaration) =>
                {
                    classDeclaration = classDeclaration.AddBaseListTypes(
                        SimpleBaseType(QualifiedName(IdentifierName("System"),
                            GenericName("IEquatable").AddTypeArgumentListArguments(
                                IdentifierName(classDeclaration.Identifier)))));

                    var fields = classDeclaration.ChildNodes()
                        .OfType<FieldDeclarationSyntax>();

                    classDeclaration = classDeclaration.ReplaceNodes(fields,
                        (__, fieldDeclaration) =>
                        {
                            var type = fieldDeclaration.Declaration.Type;
                            var name = fieldDeclaration.Declaration.Variables.Single()
                                .Identifier;

                            return PropertyDeclaration(type, name)
                                .AddModifiers(Token(PublicKeyword))
                                .AddAccessorListAccessors(
                                    AccessorDeclaration(GetAccessorDeclaration)
                                        .WithSemicolonToken(Token(SemicolonToken)),
                                    AccessorDeclaration(SetAccessorDeclaration)
                                        .WithSemicolonToken(Token(SemicolonToken)));
                        });

                    return classDeclaration;
                });

            compilationUnit = compilationUnit.NormalizeWhitespace();

            File.WriteAllText("Entities.cs", compilationUnit.ToString());
        }
    }
}
