using Microsoft.CodeAnalysis.CSharp.Syntax;
using Roslyn = Microsoft.CodeAnalysis;

namespace CSharpE.Syntax
{
    public class IdentifierExpression : Expression
    {
        public string Identifier { get; set; }

        public IdentifierExpression(string identifier) => Identifier = identifier;

        internal override ExpressionSyntax GetWrapped()
        {
            throw new System.NotImplementedException();
        }

        protected override void SetSyntaxImpl(Roslyn::SyntaxNode newSyntax)
        {
            throw new System.NotImplementedException();
        }

        internal override SyntaxNode Parent { get; set; }
    }
}