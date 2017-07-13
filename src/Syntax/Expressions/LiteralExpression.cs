namespace CSharpE.Syntax
{
    public abstract class LiteralExpression : Expression
    {
        public object Value => ValueImpl;
        
        protected abstract object ValueImpl { get; }
    }
}