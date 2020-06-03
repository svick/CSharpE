using System;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RoslynSyntaxFactory = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Roslyn = Microsoft.CodeAnalysis;

namespace CSharpE.Syntax
{
    public sealed class DiscardPattern : Pattern
    {
        private DiscardPatternSyntax syntax;

        internal DiscardPattern(DiscardPatternSyntax syntax, SyntaxNode parent)
            : base(syntax)
        {
            this.syntax = syntax;
            Parent = parent;
        }

        public DiscardPattern() { }

        protected override PatternSyntax GetWrapped(ref bool? changed)
        {
            GetAndResetChanged(ref changed, out var thisChanged);

            if (syntax == null || thisChanged == true)
            {
                syntax = RoslynSyntaxFactory.DiscardPattern();

                SetChanged(ref changed);
            }

            return syntax;
        }

        private protected override void SetSyntaxImpl(Roslyn::SyntaxNode newSyntax) => syntax = (DiscardPatternSyntax)newSyntax;

        private protected override SyntaxNode CloneImpl() => new DiscardPattern();

        public override void ReplaceExpressions<T>(Func<T, bool> filter, Func<T, Expression> projection) { }
    }
}