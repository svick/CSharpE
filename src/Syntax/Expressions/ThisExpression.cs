using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSharpE.Syntax
{
    public class ThisExpression : Expression
    {
        internal override ExpressionSyntax GetWrapped()
        {
            throw new System.NotImplementedException();
        }
    }
}