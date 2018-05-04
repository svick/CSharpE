using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSharpE.Syntax
{
    public abstract class LiteralExpression : Expression
    {
        protected LiteralExpressionSyntax Syntax;

        public object Value => ValueImpl;
        
        protected abstract object ValueImpl { get; }

        protected override IEnumerable<IEnumerable<SyntaxNode>> GetChildren() => Enumerable.Empty<IEnumerable<SyntaxNode>>();

        public override SyntaxNode Parent { get; internal set; }
    }
}