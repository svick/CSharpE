using System.Collections.Generic;
using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSharpE.Syntax
{
    public class Parameter : SyntaxNode, ISyntaxWrapper<ParameterSyntax>
    {
        public ParameterSyntax GetWrapped()
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