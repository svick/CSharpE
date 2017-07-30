using System;
using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSharpE.Syntax
{
    public abstract class MemberDefinition : ISyntaxWrapper<MemberDeclarationSyntax>
    {
        private MemberModifiers modifiers;
        public MemberModifiers Modifiers
        {
            get => modifiers;
            set
            {
                ValidateModifiers(value);
                modifiers = value;
            }
        }

        protected abstract void ValidateModifiers(MemberModifiers modifiers);

        public TypeDefinition ContainingType { get; internal set; }

        public static MemberDefinition Create(MemberDeclarationSyntax memberDeclarationSyntax, TypeDefinition containingType)
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

        protected abstract MemberDeclarationSyntax GetSyntaxImpl();
        
        public MemberDeclarationSyntax GetSyntax() => GetSyntaxImpl();

        protected abstract MemberDeclarationSyntax GetChangedSyntaxOrNullImpl();
        
        public MemberDeclarationSyntax GetChangedSyntaxOrNull() => GetChangedSyntaxOrNullImpl();
    }
}