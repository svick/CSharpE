using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Roslyn = Microsoft.CodeAnalysis;

namespace CSharpE.Syntax
{
    public class Parameter : SyntaxNode, ISyntaxWrapper<ParameterSyntax>
    {
        public ParameterSyntax GetWrapped()
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