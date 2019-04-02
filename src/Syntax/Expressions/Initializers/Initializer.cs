using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSharpE.Syntax
{
    public abstract class Initializer : SyntaxNode, ISyntaxWrapper<InitializerExpressionSyntax>
    {
        internal abstract InitializerExpressionSyntax GetWrapped(ref bool? changed);

        InitializerExpressionSyntax ISyntaxWrapper<InitializerExpressionSyntax>.GetWrapped(ref bool? changed)
            => GetWrapped(ref changed);
    }
}
