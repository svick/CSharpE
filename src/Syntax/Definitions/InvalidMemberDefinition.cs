using System;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Roslyn = Microsoft.CodeAnalysis;

namespace CSharpE.Syntax
{
    // InvalidMember is a BaseType, because this is required for NamespaceOrTypeList and doesn't hurt MemberList
    public sealed class InvalidMemberDefinition : BaseTypeDefinition
    {
        private MemberDeclarationSyntax syntax;

        internal InvalidMemberDefinition(MemberDeclarationSyntax memberSyntax, SyntaxNode parent)
            : base(memberSyntax)
        {
            syntax = memberSyntax;
            Parent = parent;
        }

        private protected override void SetSyntaxImpl(Roslyn::SyntaxNode newSyntax) =>
            syntax = (MemberDeclarationSyntax)newSyntax;

        private protected override SyntaxNode CloneImpl() => new InvalidMemberDefinition(syntax, null);

        private protected override MemberDeclarationSyntax MemberSyntax => syntax;

        private protected override void ValidateModifiers(MemberModifiers modifiers) { }

        private protected override MemberDeclarationSyntax GetWrappedMember(ref bool? changed) => syntax;

        protected override void ReplaceExpressionsImpl<T>(Func<T, bool> filter, Func<T, Expression> projection) { }
    }
}
