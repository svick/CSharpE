using System;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSharpE.Syntax
{
    public abstract class MemberDefinition
    {
        public static MemberDefinition Create(MemberDeclarationSyntax memberDeclarationSyntax, SourceFile containingFile)
        {
            switch (memberDeclarationSyntax)
            {
                case FieldDeclarationSyntax fieldDeclarationSyntax:
                    return new FieldDefinition(fieldDeclarationSyntax, containingFile);
                default:
                    throw new NotImplementedException();
            }
        }
    }
}