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

        internal override SyntaxNode Parent { get; set; }
    }
}