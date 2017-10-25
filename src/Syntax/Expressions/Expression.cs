using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSharpE.Syntax
{
    public abstract class Expression
    {
        public static implicit operator ExpressionStatement(Expression expression) => new ExpressionStatement(expression);

        // TODO: I probably need to delegate to GetWrappedImpl?
        internal abstract ExpressionSyntax GetWrapped();
    }
}