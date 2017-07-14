using System;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSharpE.Syntax
{
    public abstract class MemberDefinition
    {
        public static MemberDefinition Create(MemberDeclarationSyntax memberDeclarationSyntax, TypeDefinition containingType)
        {
            switch (memberDeclarationSyntax)
            {
                case FieldDeclarationSyntax fieldDeclarationSyntax:
                    return new FieldDefinition(fieldDeclarationSyntax, containingType);
                default:
                    throw new NotImplementedException();
            }
        }
    }
}