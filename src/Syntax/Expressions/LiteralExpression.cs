using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSharpE.Syntax
{
    public abstract class LiteralExpression : Expression, ISyntaxWrapper<LiteralExpressionSyntax>
    {
        private protected LiteralExpressionSyntax Syntax;

        public object Value => ValueImpl;
        
        protected abstract object ValueImpl { get; }

        private protected abstract LiteralExpressionSyntax GetWrappedLiteral(ref bool? changed);

        LiteralExpressionSyntax ISyntaxWrapper<LiteralExpressionSyntax>.GetWrapped(ref bool? changed) =>
            GetWrappedLiteral(ref changed);

        private protected sealed override ExpressionSyntax GetWrappedExpression(ref bool? changed) =>
            GetWrappedLiteral(ref changed);
    }
}