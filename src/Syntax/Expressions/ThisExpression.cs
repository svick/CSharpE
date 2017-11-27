using System;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using CSharpSyntaxFactory = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace CSharpE.Syntax
{
    public class ThisExpression : Expression
    {
        private ThisExpressionSyntax syntax;

        public ThisExpression() { }

        private ThisExpression(ThisExpressionSyntax syntax) =>
            this.syntax = syntax ?? throw new ArgumentNullException(nameof(syntax));

        internal override ExpressionSyntax GetWrapped()
        {
            if (syntax == null)
                syntax = CSharpSyntaxFactory.ThisExpression();

            return syntax;
        }
    }
}