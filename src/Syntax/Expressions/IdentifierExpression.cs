using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSharpE.Syntax
{
    public class IdentifierExpression : Expression
    {
        public string Identifier { get; set; }

        public IdentifierExpression(string identifier) => Identifier = identifier;

        internal override ExpressionSyntax GetWrapped(WrapperContext context)
        {
            throw new System.NotImplementedException();
        }
    }
}