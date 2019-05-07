using System;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Roslyn = Microsoft.CodeAnalysis;

namespace CSharpE.Syntax
{
    public sealed class InvalidStatement : Statement
    {
        private StatementSyntax syntax;

        internal InvalidStatement(StatementSyntax statementSyntax, SyntaxNode parent)
            : base(statementSyntax)
        {
            syntax = statementSyntax;
            Parent = parent;
        }

        private protected override void SetSyntaxImpl(Roslyn::SyntaxNode newSyntax) =>
            syntax = (StatementSyntax)newSyntax;

        private protected override SyntaxNode CloneImpl() => new InvalidStatement(syntax, null);

        private protected override StatementSyntax GetWrappedStatement(ref bool? changed) => syntax;

        public override void ReplaceExpressions<T>(Func<T, bool> filter, Func<T, Expression> projection) { }
    }
}
