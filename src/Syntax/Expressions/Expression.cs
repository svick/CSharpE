using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSharpE.Syntax
{
    public abstract class Expression : SyntaxNode, ISyntaxWrapper2<ExpressionSyntax>
    {
        public static implicit operator ExpressionStatement(Expression expression) =>
            new ExpressionStatement(expression);

        internal abstract ExpressionSyntax GetWrapped(ref bool changed);

        ExpressionSyntax ISyntaxWrapper2<ExpressionSyntax>.GetWrapped(ref bool changed) => GetWrapped(ref changed);
    }
}