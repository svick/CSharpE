using System;
using System.Collections.Generic;
using System.Linq;
using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RoslynSyntaxFactory = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Roslyn = Microsoft.CodeAnalysis;

namespace CSharpE.Syntax
{
    public sealed class WhileStatement : Statement
    {
        private WhileStatementSyntax syntax;

        internal WhileStatement(WhileStatementSyntax syntax, SyntaxNode parent) : base(syntax)
        {
            this.syntax = syntax;
            Parent = parent;
        }

        public WhileStatement(Expression condition, params Statement[] statements)
            : this(condition, statements.AsEnumerable()) { }

        public WhileStatement(Expression condition, IEnumerable<Statement> statements)
        {
            Condition = condition;
            this.statements = new StatementList(statements, this);
        }

        private Expression condition;
        public Expression Condition
        {
            get
            {
                if (condition == null)
                    condition = FromRoslyn.Expression(syntax.Condition, this);

                return condition;
            }
            set => SetNotNull(ref condition, value);
        }

        private StatementList statements;
        public IList<Statement> Statements
        {
            get
            {
                if (statements == null)
                    statements = new StatementList(GetStatementList(syntax.Statement), this);

                return statements;
            }
            set => SetList(ref statements, new StatementList(value, this));
        }

        private protected override StatementSyntax GetWrappedStatement(ref bool? changed)
        {
            GetAndResetChanged(ref changed, out var thisChanged);

            var newCondition = condition?.GetWrapped(ref thisChanged) ?? syntax.Condition;
            var newStatements = statements?.GetWrapped(ref thisChanged) ?? GetStatementList(syntax.Statement);

            if (syntax == null || thisChanged == true || ShouldAnnotate(syntax, changed))
            {
                syntax = RoslynSyntaxFactory.WhileStatement(newCondition, RoslynSyntaxFactory.Block(newStatements));

                syntax = Annotate(syntax);

                SetChanged(ref changed);
            }

            return syntax;
        }

        private protected override void SetSyntaxImpl(Roslyn::SyntaxNode newSyntax)
        {
            syntax = (WhileStatementSyntax)newSyntax;

            Set(ref condition, null);
            SetList(ref statements, null);
        }

        private protected override SyntaxNode CloneImpl() => new WhileStatement(Condition, Statements);

        public override void ReplaceExpressions<T>(Func<T, bool> filter, Func<T, Expression> projection)
        {
            Condition = Expression.ReplaceExpressions(Condition, filter, projection);

            foreach (var statement in Statements)
            {
                statement.ReplaceExpressions(filter, projection);
            }
        }
    }
}