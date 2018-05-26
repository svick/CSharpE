using System;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using CSharpSyntaxFactory = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Roslyn = Microsoft.CodeAnalysis;

namespace CSharpE.Syntax
{
    public sealed class ThisExpression : Expression
    {
        private ThisExpressionSyntax syntax;

        public ThisExpression() { }

        private ThisExpression(ThisExpressionSyntax syntax, SyntaxNode parent)
        {
            this.syntax = syntax ?? throw new ArgumentNullException(nameof(syntax));
            Parent = parent;
        }

        internal override ExpressionSyntax GetWrapped()
        {
            if (syntax == null)
                syntax = CSharpSyntaxFactory.ThisExpression();

            return syntax;
        }

        protected override void SetSyntaxImpl(Roslyn::SyntaxNode newSyntax) => syntax = (ThisExpressionSyntax)newSyntax;

        internal override SyntaxNode Parent { get; set; }
    }
}