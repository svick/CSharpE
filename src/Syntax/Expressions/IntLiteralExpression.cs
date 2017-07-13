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
    }
}