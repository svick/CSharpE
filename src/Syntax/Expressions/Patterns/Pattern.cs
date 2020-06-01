using System;
using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSharpE.Syntax
{
    public abstract class Pattern : SyntaxNode, ISyntaxWrapper<PatternSyntax>
    {
        private protected Pattern() { }
        private protected Pattern(PatternSyntax syntax) : base(syntax) { }

        PatternSyntax ISyntaxWrapper<PatternSyntax>.GetWrapped(ref bool? changed) =>
            GetWrapped(ref changed);

        protected abstract PatternSyntax GetWrapped(ref bool? changed);

        public abstract void ReplaceExpressions<T>(Func<T, bool> filter, Func<T, Expression> projection)
            where T : Expression;
    }
}