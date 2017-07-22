namespace CSharpE.Syntax
{
    public class ExpressionStatement : Statement
    {
        public Expression Expression { get; set; }

        public ExpressionStatement(Expression expression) => Expression = expression;
    }
}