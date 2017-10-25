using System;
using Microsoft.CodeAnalysis;
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
            throw new NotImplementedException();
        }

        public static MemberModifiers MemberModifiers(SyntaxTokenList modifiers)
        {
            throw new NotImplementedException();
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

    }
}