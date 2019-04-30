using System;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Roslyn = Microsoft.CodeAnalysis;

namespace CSharpE.Syntax
{
    public sealed class InvalidTypeDefinition : BaseTypeDefinition
    {
        private MemberDeclarationSyntax syntax;

        internal InvalidTypeDefinition(MemberDeclarationSyntax memberSyntax, SyntaxNode parent)
            : base(memberSyntax)
        {
            syntax = memberSyntax;
            Parent = parent;
        }

        private protected override void SetSyntaxImpl(Roslyn::SyntaxNode newSyntax) =>
            syntax = (MemberDeclarationSyntax)newSyntax;

        private protected override SyntaxNode CloneImpl() => new InvalidTypeDefinition(syntax, null);

        private protected override MemberDeclarationSyntax MemberSyntax => syntax;

        private protected override void ValidateModifiers(MemberModifiers modifiers) { }

        private protected override MemberDeclarationSyntax GetWrappedMember(ref bool? changed) => syntax;

        protected override void ReplaceExpressionsImpl<T>(Func<T, bool> filter, Func<T, Expression> projection) { }
    }
}
