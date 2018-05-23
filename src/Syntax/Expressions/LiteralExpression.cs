using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSharpE.Syntax
{
    public abstract class LiteralExpression : Expression
    {
        protected LiteralExpressionSyntax Syntax;

        public object Value => ValueImpl;
        
        protected abstract object ValueImpl { get; }

        internal override SyntaxNode Parent { get; set; }
    }
}