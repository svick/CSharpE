using System;
using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSharpE.Syntax
{
    public abstract class Initializer : SyntaxNode, ISyntaxWrapper<InitializerExpressionSyntax>
    {
        private protected Initializer() { }
        private protected Initializer(InitializerExpressionSyntax syntax) : base(syntax) { }

        internal abstract InitializerExpressionSyntax GetWrapped(ref bool? changed);

        InitializerExpressionSyntax ISyntaxWrapper<InitializerExpressionSyntax>.GetWrapped(ref bool? changed)
            => GetWrapped(ref changed);

        public abstract void ReplaceExpressions<T>(Func<T, bool> filter, Func<T, Expression> projection)
            where T : Expression;
    }
}
