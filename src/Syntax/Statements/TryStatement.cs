using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSharpE.Syntax
{
    public class TryStatement : Statement
    {
        private List<Statement> tryStatements;
        public IList<Statement> TryStatements
        {
            get => tryStatements;
            set => tryStatements = value.ToList();
        }

        private IList<CatchClause> catchClauses;
        public IList<CatchClause> CatchClauses
        {
            get => catchClauses;
            set => catchClauses = value.ToList();
        }

        private List<Statement> finallyStatements;
        public IList<Statement> FinallyStatements
        {
            get => finallyStatements;
            set => finallyStatements = value.ToList();
        }

        public TryStatement(
            IEnumerable<Statement> tryStatements, IEnumerable<CatchClause> catchClauses,
            IEnumerable<Statement> finallyStatements)
        {
            this.tryStatements = tryStatements.ToList();
            this.catchClauses = catchClauses?.ToList();
            this.finallyStatements = finallyStatements?.ToList();
        }

        public TryStatement(IEnumerable<Statement> tryStatements, IEnumerable<CatchClause> catchClauses)
            : this(tryStatements, catchClauses, null)
        {
        }

        public TryStatement(IEnumerable<Statement> tryStatements, params CatchClause[] catchClauses)
            : this(tryStatements, catchClauses, null)
        {
        }
        
        public TryStatement(IEnumerable<Statement> tryStatements, IEnumerable<Statement> finallyStatements)
            : this(tryStatements, null, finallyStatements)
        {
        }

        internal new TryStatementSyntax GetWrapped()
        {
            throw new NotImplementedException();
        }

        protected override StatementSyntax GetWrappedImpl() => GetWrapped();
    }
}