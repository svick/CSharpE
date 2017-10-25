using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSharpE.Syntax
{
    public class IntLiteralExpression : LiteralExpression
    {
        public new int Value { get; set; }

        protected override object ValueImpl => Value;
        
        public IntLiteralExpression(int value)
        {
            Value = value;
        }

        internal override ExpressionSyntax GetWrapped()
        {
            throw new System.NotImplementedException();
        }
    }
}