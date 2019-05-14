using System;
using System.Diagnostics;
using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RoslynSyntaxFactory = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Roslyn = Microsoft.CodeAnalysis;

namespace CSharpE.Syntax
{
    public sealed class GotoDefaultStatement : Statement
    {
        private GotoStatementSyntax syntax;

        internal GotoDefaultStatement(GotoStatementSyntax syntax, SyntaxNode parent)
            : base(syntax)
        {
            Debug.Assert(syntax.Kind() == SyntaxKind.GotoDefaultStatement);

            this.syntax = syntax;
            Parent = parent;
        }

        internal GotoDefaultStatement() { }

        private protected override StatementSyntax GetWrappedStatement(ref bool? changed)
        {
            GetAndResetChanged(ref changed);

            if (syntax == null)
            {
                syntax = RoslynSyntaxFactory.GotoStatement(
                    SyntaxKind.GotoDefaultStatement, RoslynSyntaxFactory.Token(SyntaxKind.DefaultKeyword), null);

                SetChanged(ref changed);
            }

            return syntax;
        }

        private protected override void SetSyntaxImpl(Roslyn::SyntaxNode newSyntax) =>
            this.syntax = (GotoStatementSyntax)newSyntax;

        private protected override SyntaxNode CloneImpl() => new GotoDefaultStatement();

        public override void ReplaceExpressions<T>(Func<T, bool> filter, Func<T, Expression> projection) { }
    }
}