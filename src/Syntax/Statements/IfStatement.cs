using System;
using System.Collections.Generic;
using System.Linq;
using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RoslynSyntaxFactory = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Roslyn = Microsoft.CodeAnalysis;

namespace CSharpE.Syntax
{
    public sealed class IfStatement : Statement
    {
        private IfStatementSyntax syntax;

        internal IfStatement(IfStatementSyntax syntax, SyntaxNode parent)
        {
            this.syntax = syntax;
            Parent = parent;
        }

        public IfStatement(Expression condition, params Statement[] thenStatements)
            : this(condition, thenStatements.AsEnumerable()) { }

        public IfStatement(Expression condition, IEnumerable<Statement> thenStatements)
            : this(condition, thenStatements, null) { }

        public IfStatement(Expression condition, IEnumerable<Statement> thenStatements, IEnumerable<Statement> elseStatements)
        {
            Condition = condition;
            this.thenStatements = new StatementList(thenStatements, this);
            this.elseStatements = new StatementList(elseStatements, this);
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

        private StatementList thenStatements;
        public IList<Statement> ThenStatements
        {
            get
            {
                if (thenStatements == null)
                    thenStatements = new StatementList(GetStatementList(syntax.Statement), this);

                return thenStatements;
            }
            set => SetList(ref thenStatements, new StatementList(value, this));
        }

        private StatementList elseStatements;
        public IList<Statement> ElseStatements
        {
            get
            {
                if (elseStatements == null)
                    elseStatements = new StatementList(GetStatementList(syntax.Else?.Statement), this);

                return elseStatements;
            }
            set => SetList(ref elseStatements, new StatementList(value, this));
        }

        private protected override StatementSyntax GetWrappedStatement(ref bool? changed)
        {
            GetAndResetChanged(ref changed);

            bool? thisChanged = false;

            var newCondition = condition?.GetWrapped(ref thisChanged) ?? syntax.Condition;
            var newThenStatements = thenStatements?.GetWrapped(ref thisChanged) ?? GetStatementList(syntax.Statement);
            var newElseStatements =
                elseStatements?.GetWrapped(ref thisChanged) ?? GetStatementList(syntax.Else?.Statement);

            if (syntax == null || thisChanged == true)
            {
                // always create block, except for "else if"
                StatementSyntax Block(Roslyn::SyntaxList<StatementSyntax> statements) =>
                    statements.Count == 1 && statements.Single() is IfStatementSyntax ifStatement
                        ? (StatementSyntax)ifStatement
                        : RoslynSyntaxFactory.Block(statements);

                var elseClause = newElseStatements.Count == 0
                    ? null
                    : RoslynSyntaxFactory.ElseClause(Block(newElseStatements));

                syntax = RoslynSyntaxFactory.IfStatement(newCondition, Block(newThenStatements), elseClause);

                SetChanged(ref changed);
            }

            return syntax;
        }

        private protected override void SetSyntaxImpl(Roslyn::SyntaxNode newSyntax)
        {
            syntax = (IfStatementSyntax)newSyntax;

            Set(ref condition, null);
            SetList(ref thenStatements, null);
            SetList(ref elseStatements, null);
        }

        private protected override SyntaxNode CloneImpl() => new IfStatement(Condition, ThenStatements, ElseStatements);

        public override IEnumerable<SyntaxNode> GetChildren() =>
            new SyntaxNode[] { Condition }.Concat(ThenStatements).Concat(ElseStatements);

        public override void ReplaceExpressions<T>(Func<T, bool> filter, Func<T, Expression> projection)
        {
            Condition = Expression.ReplaceExpressions(Condition, filter, projection);

            foreach (var thenStatement in ThenStatements)
            {
                thenStatement.ReplaceExpressions(filter, projection);
            }

            foreach (var elseStatement in ElseStatements)
            {
                elseStatement.ReplaceExpressions(filter, projection);
            }
        }
    }
}