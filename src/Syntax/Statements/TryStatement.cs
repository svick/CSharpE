using System.Collections.Generic;
using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using CSharpSyntaxFactory = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

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
            this.tryStatements = new SyntaxList<Statement, StatementSyntax>(tryStatements);
            this.catchClauses = new SyntaxList<CatchClause, CatchClauseSyntax>(catchClauses);
            this.finallyStatements = new SyntaxList<Statement, StatementSyntax>(finallyStatements);
        }

        public TryStatement(IEnumerable<Statement> tryStatements, IEnumerable<CatchClause> catchClauses)
            : this(tryStatements, catchClauses, null) { }

        public TryStatement(IEnumerable<Statement> tryStatements, params CatchClause[] catchClauses)
            : this(tryStatements, catchClauses, null) { }

        public TryStatement(IEnumerable<Statement> tryStatements, IEnumerable<Statement> finallyStatements)
            : this(tryStatements, null, finallyStatements) { }

        private SyntaxList<Statement, StatementSyntax> tryStatements;
        public IList<Statement> TryStatements
        {
            get
            {
                if (tryStatements == null)
                    tryStatements = new SyntaxList<Statement, StatementSyntax>(syntax.Block.Statements);

                return tryStatements;
            }
            set => tryStatements = new SyntaxList<Statement, StatementSyntax>(value);
        }

        private SyntaxList<CatchClause, CatchClauseSyntax> catchClauses;
        public IList<CatchClause> CatchClauses
        {
            get
            {
                if (catchClauses == null)
                    catchClauses = new SyntaxList<CatchClause, CatchClauseSyntax>(syntax.Catches);

                return catchClauses;
            }
            set => catchClauses = new SyntaxList<CatchClause, CatchClauseSyntax>();
        }

        private SyntaxList<Statement, StatementSyntax> finallyStatements;
        public IList<Statement> FinallyStatements
        {
            get
            {
                if (finallyStatements == null)
                    finallyStatements = new SyntaxList<Statement, StatementSyntax>(syntax.Finally?.Block.Statements ?? default);

                return finallyStatements;
            }
            set => finallyStatements = new SyntaxList<Statement, StatementSyntax>(value);
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

        protected override IEnumerable<IEnumerable<SyntaxNode>> GetChildren()
        {
            yield return TryStatements;
            yield return CatchClauses;
            yield return FinallyStatements;
        }

        internal override SyntaxNode Parent { get; set; }
    }
}