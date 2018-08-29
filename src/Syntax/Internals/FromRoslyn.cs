using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSharpE.Syntax.Internals
{
    /// <summary>
    /// Converts values from Roslyn's object model to CSharpE's object model.
    /// </summary>
    /// <remarks>
    /// Many CSharpE types have constructors that take Roslyn types, those might not have methods in this type.
    /// </remarks>
    internal static class FromRoslyn
    {
        public static Expression Expression(ExpressionSyntax syntax, SyntaxNode parent)
        {
            switch (syntax)
            {
                case LiteralExpressionSyntax literalSyntax: return LiteralExpression(literalSyntax, parent);
                case null: return null;
            }

            throw new NotImplementedException();
        }

        public static LiteralExpression LiteralExpression(LiteralExpressionSyntax syntax, SyntaxNode parent)
        {
            switch (syntax.Kind())
            {
                case SyntaxKind.NumericLiteralExpression:
                    if (syntax.Token.Value is int)
                        return new IntLiteralExpression(syntax, parent);
                    break;
            }

            throw new NotImplementedException();
        }

        public static Statement Statement(StatementSyntax syntax, SyntaxNode parent)
        {
            switch (syntax)
            {
                case null:
                    return null;
                case ReturnStatementSyntax returnStatement:
                    return new ReturnStatement(returnStatement, parent);
            }

            throw new NotImplementedException();
        }

        public static MemberModifiers MemberModifiers(SyntaxTokenList modifiers)
        {
            MemberModifiers result = 0;

            foreach (var modifier in modifiers)
            {
                result |= MemberModifiersExtensions.ModifiersMapping[modifier.Kind()];
            }

            return result;
        }

        public static MemberDefinition MemberDefinition(MemberDeclarationSyntax memberDeclarationSyntax, TypeDefinition containingType)
        {
            switch (memberDeclarationSyntax)
            {
                case FieldDeclarationSyntax fieldDeclarationSyntax:
                    return new FieldDefinition(fieldDeclarationSyntax, containingType);
                case MethodDeclarationSyntax methodDeclarationSyntax:
                    return new MethodDefinition(methodDeclarationSyntax, containingType);
                case ClassDeclarationSyntax classDeclarationSyntax:
                    return new TypeDefinition(classDeclarationSyntax, containingType);
                default:
                    throw new NotImplementedException();
            }
        }

        public static TypeReference TypeReference(TypeSyntax typeSyntax, SyntaxNode parent)
        {
            return new NamedTypeReference(typeSyntax, parent);
        }
    }
}