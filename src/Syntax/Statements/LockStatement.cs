using System;
using System.Collections.Generic;
using System.Linq;
using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RoslynSyntaxFactory = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Roslyn = Microsoft.CodeAnalysis;

namespace CSharpE.Syntax
{
    public sealed class LockStatement : Statement
    {
        private LockStatementSyntax syntax;

        internal LockStatement(LockStatementSyntax syntax, SyntaxNode parent) : base(syntax)
        {
            this.syntax = syntax;
            Parent = parent;
        }

        public LockStatement(Expression expression, params Statement[] statements)
            : this(expression, statements.AsEnumerable()) { }

        public LockStatement(Expression expression, IEnumerable<Statement> statements)
        {
            Expression = expression;
            this.statements = new StatementList(statements, this);
        }

        private Expression expression;
        public Expression Expression
        {
            get
            {
                if (expression == null)
                    expression = FromRoslyn.Expression(syntax.Expression, this);

                return expression;
            }
            set => SetNotNull(ref expression, value);
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
            GetAndResetChanged(ref changed);

            bool? thisChanged = false;

            var newExpression = expression?.GetWrapped(ref thisChanged) ?? syntax.Expression;
            var newStatements = statements?.GetWrapped(ref thisChanged) ?? GetStatementList(syntax.Statement);

            if (syntax == null || thisChanged == true)
            {
                syntax = RoslynSyntaxFactory.LockStatement(newExpression, RoslynSyntaxFactory.Block(newStatements));

                SetChanged(ref changed);
            }

            return syntax;
        }

        private protected override void SetSyntaxImpl(Roslyn::SyntaxNode newSyntax)
        {
            syntax = (LockStatementSyntax)newSyntax;

            Set(ref expression, null);
            SetList(ref statements, null);
        }

        private protected override SyntaxNode CloneImpl() => new LockStatement(Expression, Statements);

        public override void ReplaceExpressions<T>(Func<T, bool> filter, Func<T, Expression> projection)
        {
            Expression = Expression.ReplaceExpressions(Expression, filter, projection);

            foreach (var statement in Statements)
            {
                statement.ReplaceExpressions(filter, projection);
            }
        }
    }
}