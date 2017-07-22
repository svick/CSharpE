namespace CSharpE.Syntax
{
    public abstract class Expression
    {
        public static implicit operator ExpressionStatement(Expression expression) => new ExpressionStatement(expression);
    }
}