using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSharpE.Syntax
{
    public abstract class Statement : ISyntaxWrapper<StatementSyntax>
    {
        public StatementSyntax GetWrapped()
        {
            throw new System.NotImplementedException();
        }
    }
}