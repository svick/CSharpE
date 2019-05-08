using System;
using System.Collections.Generic;
using System.Linq;
using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RoslynSyntaxFactory = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Roslyn = Microsoft.CodeAnalysis;

namespace CSharpE.Syntax
{
    public sealed class DoWhileStatement : Statement
    {
        private DoStatementSyntax syntax;

        internal DoWhileStatement(DoStatementSyntax syntax, SyntaxNode parent) : base(syntax)
        {
            this.syntax = syntax;
            Parent = parent;
        }

        public DoWhileStatement(IEnumerable<Statement> statements, Expression condition)
        {
            this.statements = new StatementList(statements, this);
            Condition = condition;
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

        private protected override StatementSyntax GetWrappedStatement(ref bool? changed)
        {
            GetAndResetChanged(ref changed);

            bool? thisChanged = false;

            var newStatements = statements?.GetWrapped(ref thisChanged) ?? GetStatementList(syntax.Statement);
            var newCondition = condition?.GetWrapped(ref thisChanged) ?? syntax.Condition;

            if (syntax == null || thisChanged == true || ShouldAnnotate(syntax, changed))
            {
                syntax = RoslynSyntaxFactory.DoStatement(RoslynSyntaxFactory.Block(newStatements), newCondition);

                syntax = Annotate(syntax);

                SetChanged(ref changed);
            }

            return syntax;
        }

        private protected override void SetSyntaxImpl(Roslyn::SyntaxNode newSyntax)
        {
            syntax = (DoStatementSyntax)newSyntax;

            SetList(ref statements, null);
            Set(ref condition, null);
        }

        private protected override SyntaxNode CloneImpl() => new DoWhileStatement(Statements, Condition);

        public override IEnumerable<SyntaxNode> GetChildren() => Statements.Concat(new SyntaxNode[] { Condition });

        public override void ReplaceExpressions<T>(Func<T, bool> filter, Func<T, Expression> projection)
        {
            foreach (var statement in Statements)
            {
                statement.ReplaceExpressions(filter, projection);
            }

            Condition = Expression.ReplaceExpressions(Condition, filter, projection);
        }
    }
}