using System;
using System.Collections.Generic;
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

            compilationUnit =
                compilationUnit.AddUsings(UsingDirective(IdentifierName("System")));

            compilationUnit = compilationUnit.ReplaceNodes(
                compilationUnit.DescendantNodes().OfType<ClassDeclarationSyntax>(),
                (_, classDeclaration) =>
                {
                    classDeclaration = classDeclaration.AddBaseListTypes(
                        SimpleBaseType(GenericName("IComparable")
                            .AddTypeArgumentListArguments(
                                IdentifierName(classDeclaration.Identifier))));

                    var fields = classDeclaration.ChildNodes()
                        .OfType<FieldDeclarationSyntax>().ToList();

                    classDeclaration = classDeclaration.ReplaceNodes(fields,
                        (__, fieldDeclaration) =>
                        {
                            var type = fieldDeclaration.Declaration.Type;
                            var name = fieldDeclaration.Declaration.Variables.Single()
                                .Identifier;

                            var property = PropertyDeclaration(type, name)
                                .AddModifiers(Token(PublicKeyword))
                                .AddAccessorListAccessors(
                                    AccessorDeclaration(GetAccessorDeclaration)
                                        .WithSemicolonToken(Token(SemicolonToken)),
                                    AccessorDeclaration(SetAccessorDeclaration)
                                        .WithSemicolonToken(Token(SemicolonToken)));

                            return property;
                        });

                    var statements = new List<StatementSyntax>();

                    statements.Add(LocalDeclarationStatement(
                        VariableDeclaration(PredefinedType(Token(IntKeyword)))
                            .AddVariables(VariableDeclarator("result"))));

                    foreach (var field in fields)
                    {
                        var fieldIdentifier = IdentifierName(
                            field.Declaration.Variables.Single().Identifier);

                        statements.Add(ExpressionStatement(AssignmentExpression(
                            SimpleAssignmentExpression, IdentifierName("result"),
                            InvocationExpression(MemberAccessExpression(
                                SimpleMemberAccessExpression, fieldIdentifier,
                                IdentifierName("CompareTo"))).AddArgumentListArguments(
                                Argument(MemberAccessExpression(
                                    SimpleMemberAccessExpression, IdentifierName("other"),
                                    fieldIdentifier))))));

                        statements.Add(IfStatement(
                            BinaryExpression(NotEqualsExpression,
                                IdentifierName("result"),
                                LiteralExpression(NumericLiteralExpression, Literal(0))),
                            ReturnStatement(IdentifierName("result"))));
                    }

                    statements.Add(ReturnStatement(
                        LiteralExpression(NumericLiteralExpression, Literal(0))));

                    var compareToMethod =
                        MethodDeclaration(PredefinedType(Token(IntKeyword)), "CompareTo")
                            .AddModifiers(Token(PublicKeyword))
                            .AddParameterListParameters(Parameter(Identifier("other"))
                                .WithType(IdentifierName(classDeclaration.Identifier)))
                            .WithBody(Block(statements));
                    classDeclaration = classDeclaration.AddMembers(compareToMethod);

                    return classDeclaration;
                });

            compilationUnit = compilationUnit.NormalizeWhitespace();

            Console.WriteLine(compilationUnit);
        }
    }
}
