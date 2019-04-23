using System;
using System.Collections.Generic;
using System.Linq;
using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RoslynSyntaxFactory = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Roslyn = Microsoft.CodeAnalysis;

namespace CSharpE.Syntax
{
    public sealed class TryStatement : Statement
    {
        private TryStatementSyntax syntax;

        internal TryStatement(TryStatementSyntax syntax, SyntaxNode parent)
        {
            this.syntax = syntax;
            Parent = parent;
        }

        public TryStatement(
            IEnumerable<Statement> tryStatements, IEnumerable<CatchClause> catchClauses,
            IEnumerable<Statement> finallyStatements)
        {
            this.tryStatements = new StatementList(tryStatements, this);
            this.catchClauses = new SyntaxList<CatchClause, CatchClauseSyntax>(catchClauses, this);
            this.finallyStatements = new StatementList(finallyStatements, this);
        }

        public TryStatement(IEnumerable<Statement> tryStatements, IEnumerable<CatchClause> catchClauses)
            : this(tryStatements, catchClauses, null) { }

        public TryStatement(IEnumerable<Statement> tryStatements, params CatchClause[] catchClauses)
            : this(tryStatements, catchClauses, null) { }

        public TryStatement(IEnumerable<Statement> tryStatements, IEnumerable<Statement> finallyStatements)
            : this(tryStatements, null, finallyStatements) { }

        private StatementList tryStatements;
        public IList<Statement> TryStatements
        {
            get
            {
                if (tryStatements == null)
                    tryStatements = new StatementList(syntax.Block.Statements, this);

                return tryStatements;
            }
            set => SetList(ref tryStatements, new StatementList(value, this));
        }

        private SyntaxList<CatchClause, CatchClauseSyntax> catchClauses;
        public IList<CatchClause> CatchClauses
        {
            get
            {
                if (catchClauses == null)
                    catchClauses = new SyntaxList<CatchClause, CatchClauseSyntax>(syntax.Catches, this);

                return catchClauses;
            }
            set => SetList(ref catchClauses, new SyntaxList<CatchClause, CatchClauseSyntax>(value, this));
        }

        private StatementList finallyStatements;
        public IList<Statement> FinallyStatements
        {
            get
            {
                if (finallyStatements == null)
                    finallyStatements = new StatementList(syntax.Finally?.Block.Statements ?? default, this);

                return finallyStatements;
            }
            set => SetList(ref finallyStatements, new StatementList(value, this));
        }

        private protected override StatementSyntax GetWrappedStatement(ref bool? changed)
        {
            GetAndResetChanged(ref changed);

            bool? thisChanged = false;

            var newTryStatements = tryStatements?.GetWrapped(ref thisChanged) ?? syntax.Block.Statements;
            var newCatchClauses = catchClauses?.GetWrapped(ref thisChanged) ?? syntax.Catches;
            var newFinallyStatements = finallyStatements?.GetWrapped(ref thisChanged) ?? syntax.Finally?.Block.Statements ?? default;

            if (syntax == null || thisChanged == true)
            {
                var newFinallyClause = newFinallyStatements.Any()
                    ? RoslynSyntaxFactory.FinallyClause(RoslynSyntaxFactory.Block(newFinallyStatements))
                    : null;

                syntax = RoslynSyntaxFactory.TryStatement(
                    RoslynSyntaxFactory.Block(newTryStatements), newCatchClauses, newFinallyClause);

                SetChanged(ref changed);
            }

            return syntax;
        }

        private protected override void SetSyntaxImpl(Roslyn::SyntaxNode newSyntax)
        {
            syntax = (TryStatementSyntax)newSyntax;

            tryStatements = null;
            catchClauses = null;
            finallyStatements = null;
        }

        internal override SyntaxNode Clone() => new TryStatement(TryStatements, CatchClauses, FinallyStatements);

        internal override SyntaxNode Parent { get; set; }

        public override IEnumerable<SyntaxNode> GetChildren() =>
            TryStatements.Concat<SyntaxNode>(CatchClauses).Concat(FinallyStatements);

        public override void ReplaceExpressions<T>(Func<T, bool> filter, Func<T, Expression> projection)
        {
            foreach (var tryStatement in TryStatements)
            {
                tryStatement.ReplaceExpressions(filter, projection);
            }

            foreach (var catchClause in CatchClauses)
            {
                catchClause.Filter = Expression.ReplaceExpressions(catchClause.Filter, filter, projection);
            }

            foreach (var finallyStatement in FinallyStatements)
            {
                finallyStatement.ReplaceExpressions(filter, projection);
            }
        }
    }
}