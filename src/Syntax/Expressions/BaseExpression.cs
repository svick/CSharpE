﻿using Microsoft.CodeAnalysis.CSharp.Syntax;
using RoslynSyntaxFactory = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Roslyn = Microsoft.CodeAnalysis;

namespace CSharpE.Syntax
{
    public sealed class BaseExpression : Expression
    {
        private BaseExpressionSyntax syntax;

        internal BaseExpression(BaseExpressionSyntax syntax, SyntaxNode parent)
        {
            this.syntax = syntax;
            Parent = parent;
        }

        public BaseExpression() { }

        private protected override ExpressionSyntax GetWrappedExpression(ref bool? changed)
        {
            GetAndResetChanged(ref changed);

            if (syntax == null)
            {
                syntax = RoslynSyntaxFactory.BaseExpression();

                SetChanged(ref changed);
            }

            return syntax;
        }

        private protected override void SetSyntaxImpl(Roslyn::SyntaxNode newSyntax) =>
            syntax = (BaseExpressionSyntax)newSyntax;

        internal override SyntaxNode Clone() => new BaseExpression();

        internal override SyntaxNode Parent { get; set; }
    }
}