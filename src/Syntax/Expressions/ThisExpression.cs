using System;
using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RoslynSyntaxFactory = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Roslyn = Microsoft.CodeAnalysis;

namespace CSharpE.Syntax
{
    public sealed class ThisExpression : Expression, ISyntaxWrapper<ThisExpressionSyntax>
    {
        private ThisExpressionSyntax syntax;

        public ThisExpression() { }

        internal ThisExpression(ThisExpressionSyntax syntax, SyntaxNode parent)
        {
            this.syntax = syntax ?? throw new ArgumentNullException(nameof(syntax));
            Parent = parent;
        }

        ThisExpressionSyntax ISyntaxWrapper<ThisExpressionSyntax>.GetWrapped(ref bool? changed)
        {
            GetAndResetChanged(ref changed);

            if (syntax == null)
            {
                syntax = RoslynSyntaxFactory.ThisExpression();

                SetChanged(ref changed);
            }

            return syntax;
        }

        private protected override ExpressionSyntax GetWrappedExpression(ref bool? changed) =>
            this.GetWrapped<ThisExpressionSyntax>(ref changed);

        private protected override void SetSyntaxImpl(Roslyn::SyntaxNode newSyntax) => syntax = (ThisExpressionSyntax)newSyntax;

        internal override SyntaxNode Clone() => new ThisExpression();

        internal override SyntaxNode Parent { get; set; }
    }
}