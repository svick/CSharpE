using System;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Roslyn = Microsoft.CodeAnalysis;

namespace CSharpE.Syntax
{
    public sealed class IncompleteMemberDefinition : MemberDefinition
    {
        private IncompleteMemberSyntax syntax;

        internal IncompleteMemberDefinition(IncompleteMemberSyntax incompleteMemberSyntax, SyntaxNode parent)
            : base(incompleteMemberSyntax)
        {
            syntax = incompleteMemberSyntax;
            Parent = parent;
        }

        private protected override void SetSyntaxImpl(Roslyn::SyntaxNode newSyntax) =>
            syntax = (IncompleteMemberSyntax)newSyntax;

        internal override SyntaxNode Clone() => new InvalidTypeDefinition(syntax, null);

        private protected override MemberDeclarationSyntax MemberSyntax => syntax;

        private protected override void ValidateModifiers(MemberModifiers modifiers) { }

        private protected override MemberDeclarationSyntax GetWrappedMember(ref bool? changed) => syntax;

        protected override void ReplaceExpressionsImpl<T>(Func<T, bool> filter, Func<T, Expression> projection) { }
    }
}
