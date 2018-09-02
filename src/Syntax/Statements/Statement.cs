using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSharpE.Syntax
{
    public abstract class Statement : SyntaxNode, ISyntaxWrapper<StatementSyntax>
    {
        StatementSyntax ISyntaxWrapper<StatementSyntax>.GetWrapped(ref bool? changed) => GetWrappedStatement(ref changed);

        private protected abstract StatementSyntax GetWrappedStatement(ref bool? changed);
    }
}