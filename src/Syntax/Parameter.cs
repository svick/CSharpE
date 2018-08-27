using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Roslyn = Microsoft.CodeAnalysis;

namespace CSharpE.Syntax
{
    public class Parameter : SyntaxNode, ISyntaxWrapper<ParameterSyntax>
    {
        internal ParameterSyntax GetWrapped(ref bool? changed)
        {
            throw new System.NotImplementedException();
        }

        ParameterSyntax ISyntaxWrapper<ParameterSyntax>.GetWrapped(ref bool? changed) => GetWrapped(ref changed);

        private protected override void SetSyntaxImpl(Roslyn::SyntaxNode newSyntax)
        {
            throw new System.NotImplementedException();
        }

        internal override SyntaxNode Clone()
        {
            throw new System.NotImplementedException();
        }

        internal override SyntaxNode Parent { get; set; }
    }
}