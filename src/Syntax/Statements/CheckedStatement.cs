using System;
using System.Collections.Generic;
using System.Linq;
using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RoslynSyntaxFactory = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Roslyn = Microsoft.CodeAnalysis;

namespace CSharpE.Syntax
{
    public sealed class CheckedStatement : Statement
    {
        private CheckedStatementSyntax syntax;

        internal CheckedStatement(CheckedStatementSyntax syntax, SyntaxNode parent)
            : base(syntax)
        {
            Init(syntax);
            Parent = parent;
        }

        private void Init(CheckedStatementSyntax syntax)
        {
            this.syntax = syntax;
            IsChecked = IsSyntaxChecked();
        }

        public CheckedStatement(bool isChecked, params Statement[] statements)
            : this(isChecked, statements.AsEnumerable()) { }

        public CheckedStatement(bool isChecked, IEnumerable<Statement> statements)
        {
            IsChecked = isChecked;
            this.statements = new StatementList(statements, this);
        }

        private bool IsSyntaxChecked() => syntax.Kind() == SyntaxKind.CheckedStatement;

        public bool IsChecked { get; set; }

        private StatementList statements;
        public IList<Statement> Statements
        {
            get
            {
                if (statements == null)
                    statements = new StatementList(syntax.Block.Statements, this);

                return statements;
            }
            set => SetList(ref statements, new StatementList(value, this));
        }

        private protected override StatementSyntax GetWrappedStatement(ref bool? changed)
        {
            GetAndResetChanged(ref changed, out var thisChanged);

            var newStatements = statements?.GetWrapped(ref thisChanged) ?? syntax.Block.Statements;

            bool isCheckedChanged = syntax != null && IsSyntaxChecked() != IsChecked;

            if (syntax == null || thisChanged == true || isCheckedChanged)
            {
                syntax = RoslynSyntaxFactory.CheckedStatement(
                    IsChecked ? SyntaxKind.CheckedStatement : SyntaxKind.UncheckedStatement,
                    RoslynSyntaxFactory.Block(newStatements));

                SetChanged(ref changed);
            }

            return syntax;
        }

        private protected override void SetSyntaxImpl(Roslyn::SyntaxNode newSyntax)
        {
            SetList(ref statements, null);
            syntax = (CheckedStatementSyntax)newSyntax;
        }

        private protected override SyntaxNode CloneImpl() => new CheckedStatement(IsChecked, Statements);

        public override void ReplaceExpressions<T>(Func<T, bool> filter, Func<T, Expression> projection)
        {
            foreach (var statement in Statements)
            {
                statement.ReplaceExpressions(filter, projection);
            }
        }
    }
}