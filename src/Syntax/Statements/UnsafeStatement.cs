using System;
using System.Collections.Generic;
using System.Linq;
using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RoslynSyntaxFactory = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Roslyn = Microsoft.CodeAnalysis;

namespace CSharpE.Syntax
{
    public sealed class UnsafeStatement : Statement
    {
        private UnsafeStatementSyntax syntax;

        internal UnsafeStatement(UnsafeStatementSyntax syntax, SyntaxNode parent)
            : base(syntax)
        {
            this.syntax = syntax;
            Parent = parent;
        }

        public UnsafeStatement(params Statement[] statements) : this(statements.AsEnumerable()) { }

        public UnsafeStatement(IEnumerable<Statement> statements) =>
            this.statements = new StatementList(statements, this);

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
            GetAndResetChanged(ref changed);

            bool? thisChanged = false;

            var newStatements = statements?.GetWrapped(ref thisChanged) ?? syntax.Block.Statements;

            if (syntax == null || thisChanged == true)
            {
                syntax = RoslynSyntaxFactory.UnsafeStatement(RoslynSyntaxFactory.Block(newStatements));

                SetChanged(ref changed);
            }

            return syntax;
        }

        private protected override void SetSyntaxImpl(Roslyn::SyntaxNode newSyntax)
        {
            SetList(ref statements, null);
            syntax = (UnsafeStatementSyntax)newSyntax;
        }

        private protected override SyntaxNode CloneImpl() => new UnsafeStatement(Statements);

        public override void ReplaceExpressions<T>(Func<T, bool> filter, Func<T, Expression> projection)
        {
            foreach (var statement in Statements)
            {
                statement.ReplaceExpressions(filter, projection);
            }
        }
    }
}