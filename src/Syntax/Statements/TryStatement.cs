using System.Collections.Generic;
using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using CSharpSyntaxFactory = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Roslyn = Microsoft.CodeAnalysis;

namespace CSharpE.Syntax
{
    public class TryStatement : Statement
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
            set => SetList(ref catchClauses, new SyntaxList<CatchClause, CatchClauseSyntax>(this));
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

        internal new TryStatementSyntax GetWrapped()
        {
            var newTryStatements = tryStatements?.GetWrapped() ?? syntax.Block.Statements;
            var newCatchClauses = catchClauses?.GetWrapped() ?? syntax.Catches;
            var newFinallyStatements = finallyStatements?.GetWrapped() ?? syntax.Finally?.Block.Statements ?? default;

            if (syntax == null || newTryStatements != syntax.Block.Statements || newCatchClauses != syntax.Catches ||
                newFinallyStatements != (syntax.Finally?.Block.Statements ?? default))
            {
                var newFinallyClause = newFinallyStatements.Any()
                    ? CSharpSyntaxFactory.FinallyClause(CSharpSyntaxFactory.Block(newFinallyStatements))
                    : null;

                syntax = CSharpSyntaxFactory.TryStatement(
                    CSharpSyntaxFactory.Block(newTryStatements), newCatchClauses, newFinallyClause);
            }

            return syntax;
        }

        protected override StatementSyntax GetWrappedImpl() => GetWrapped();

        protected override void SetSyntaxImpl(Roslyn::SyntaxNode newSyntax)
        {
            syntax = (TryStatementSyntax)newSyntax;

            tryStatements = null;
            catchClauses = null;
            finallyStatements = null;
        }

        internal override SyntaxNode Clone() => new TryStatement(TryStatements, CatchClauses, FinallyStatements);

        internal override SyntaxNode Parent { get; set; }
    }
}