using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSharpE.Syntax
{
    public class Attribute : ISyntaxWrapper<AttributeSyntax>
    {
        private AttributeSyntax syntax;

        public Attribute(AttributeSyntax syntax) => this.syntax = syntax;

        public AttributeSyntax GetWrapped(WrapperContext context)
        {
            throw new System.NotImplementedException();
        }
    }
}