using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSharpE.Syntax
{
    public abstract class Expression : SyntaxNode, ISyntaxWrapper<ExpressionSyntax>
    {
        public static implicit operator ExpressionStatement(Expression expression) =>
            new ExpressionStatement(expression);

        internal abstract ExpressionSyntax GetWrapped();

        ExpressionSyntax ISyntaxWrapper<ExpressionSyntax>.GetWrapped() => GetWrapped();
    }
}