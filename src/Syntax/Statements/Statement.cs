using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSharpE.Syntax
{
    public abstract class Statement : ISyntaxWrapper<StatementSyntax>
    {
        StatementSyntax ISyntaxWrapper<StatementSyntax>.GetWrapped(WrapperContext context) => GetWrapped(context);

        internal StatementSyntax GetWrapped(WrapperContext context) => GetWrappedImpl(context);

        protected abstract StatementSyntax GetWrappedImpl(WrapperContext context);
    }
}