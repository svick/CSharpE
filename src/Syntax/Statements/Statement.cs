using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSharpE.Syntax
{
    public abstract class Statement : ISyntaxWrapper<StatementSyntax>
    {
        StatementSyntax ISyntaxWrapper<StatementSyntax>.GetWrapped() => GetWrapped();

        internal StatementSyntax GetWrapped() => GetWrappedImpl();

        protected abstract StatementSyntax GetWrappedImpl();
    }
}