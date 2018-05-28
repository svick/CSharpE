using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSharpE.Syntax
{
    public abstract class Statement : SyntaxNode, ISyntaxWrapper<StatementSyntax>
    {
        StatementSyntax ISyntaxWrapper<StatementSyntax>.GetWrapped(ref bool? changed) => GetWrappedImpl(ref changed);

        protected abstract StatementSyntax GetWrappedImpl(ref bool? changed);
    }
}