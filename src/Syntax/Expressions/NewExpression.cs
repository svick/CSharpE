namespace CSharpE.Syntax
{
    public class NewExpression : Expression
    {
        private TypeReference type;
        private Expression[] arguments;

        public NewExpression(TypeReference type, params Expression[] arguments)
        {
            this.type = type;
            this.arguments = arguments;
        }
    }
}