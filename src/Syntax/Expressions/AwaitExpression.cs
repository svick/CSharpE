using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSharpE.Syntax
{
    public class AwaitExpression : Expression
    {
        public Expression Operand { get; set; }
        
        public AwaitExpression(Expression operand) => this.Operand = operand;

        internal override ExpressionSyntax GetWrapped()
        {
            throw new System.NotImplementedException();
        }
    }
}