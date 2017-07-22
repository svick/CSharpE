namespace CSharpE.Syntax
{
    public class AwaitExpression : Expression
    {
        public Expression Operand { get; set; }
        
        public AwaitExpression(Expression operand) => this.Operand = operand;
    }
}