using System;
using CSharpE.Syntax.Internals;
using Roslyn = Microsoft.CodeAnalysis;

namespace CSharpE.Syntax
{
    public abstract class LinqClause : SyntaxNode, ISyntaxWrapper<Roslyn::SyntaxNode>
    {
        private protected LinqClause() { }
        private protected LinqClause(Roslyn::SyntaxNode syntax) : base(syntax) { }

        internal abstract Roslyn::SyntaxNode GetWrapped(ref bool? changed);

        Roslyn::SyntaxNode ISyntaxWrapper<Roslyn::SyntaxNode>.GetWrapped(ref bool? changed)
            => GetWrapped(ref changed);

        public abstract void ReplaceExpressions<T>(Func<T, bool> filter, Func<T, Expression> projection)
            where T : Expression;
    }
}
