using System.Collections.Generic;
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

        protected override IEnumerable<IEnumerable<SyntaxNode>> GetChildren()
        {
            throw new System.NotImplementedException();
        }

        public override SyntaxNode Parent { get; internal set; }
    }
}