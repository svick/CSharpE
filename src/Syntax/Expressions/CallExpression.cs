using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSharpE.Syntax
{
    public class CallExpression : Expression
    {
        private Expression receiver;
        private string methodName;
        private Expression[] arguments;

        public CallExpression(Expression receiver, string methodName, Expression[] arguments)
        {
            this.receiver = receiver;
            this.methodName = methodName;
            this.arguments = arguments;
        }

        internal override ExpressionSyntax GetWrapped()
        {
            throw new System.NotImplementedException();
        }
    }
}