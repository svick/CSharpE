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
        public static Expression Expression(ExpressionSyntax syntax)
        {
            switch (syntax)
            {
                case null:
                    return null;
            }

            throw new NotImplementedException();
        }

        public static Statement Statement(StatementSyntax syntax)
        {
            switch (syntax)
            {
                case null:
                    return null;
                case ReturnStatementSyntax returnStatement:
                    return new ReturnStatement(returnStatement);
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
                default:
                    throw new NotImplementedException();
            }
        }

        public static TypeReference TypeReference(TypeSyntax typeSyntax, SyntaxContext context)
        {
            return new NamedTypeReference(typeSyntax, context);
        }
    }
}