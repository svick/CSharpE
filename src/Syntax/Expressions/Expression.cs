using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSharpE.Syntax
{
    public abstract class Expression : SyntaxNode, ISyntaxWrapper<ExpressionSyntax>
    {
        public static implicit operator ExpressionStatement(Expression expression) =>
            new ExpressionStatement(expression);

        private protected abstract ExpressionSyntax GetWrappedExpression(ref bool? changed);

        ExpressionSyntax ISyntaxWrapper<ExpressionSyntax>.GetWrapped(ref bool? changed) =>
            GetWrappedExpression(ref changed);
    }
}