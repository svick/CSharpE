using System;
using System.Diagnostics;
using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RoslynSyntaxFactory = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Roslyn = Microsoft.CodeAnalysis;

namespace CSharpE.Syntax
{
    public sealed class GotoStatement : Statement
    {
        private GotoStatementSyntax syntax;

        internal GotoStatement(GotoStatementSyntax syntax, SyntaxNode parent)
            : base(syntax)
        {
            Debug.Assert(syntax.Kind() == SyntaxKind.GotoStatement);

            Init(syntax);
            Parent = parent;
        }

        private void Init(GotoStatementSyntax syntax)
        {
            this.syntax = syntax;
            label = new Identifier(((IdentifierNameSyntax)syntax.Expression).Identifier);
        }

        public GotoStatement(string label) => this.label = new Identifier(label);

        private Identifier label;
        public string Label
        {
            get => label.Text;
            set => label.Text = value;
        }

        private protected override StatementSyntax GetWrappedStatement(ref bool? changed)
        {
            GetAndResetChanged(ref changed);

            bool? thisChanged = false;

            var newLabel = label.GetWrapped(ref thisChanged);

            if (syntax == null || thisChanged == true)
            {
                syntax = RoslynSyntaxFactory.GotoStatement(
                    SyntaxKind.GotoStatement, RoslynSyntaxFactory.IdentifierName(newLabel));

                SetChanged(ref changed);
            }

            return syntax;
        }

        private protected override void SetSyntaxImpl(Roslyn::SyntaxNode newSyntax) =>
            Init((GotoStatementSyntax)newSyntax);

        private protected override SyntaxNode CloneImpl() => new GotoStatement(Label);

        public override void ReplaceExpressions<T>(Func<T, bool> filter, Func<T, Expression> projection) { }
    }
}