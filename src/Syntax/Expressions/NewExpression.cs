using Microsoft.CodeAnalysis.CSharp.Syntax;

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

        internal override ExpressionSyntax GetWrapped()
        {
            throw new System.NotImplementedException();
        }
    }
}