using System;
using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSharpE.Syntax
{
    public class ReturnStatement : Statement
    {
        private StatementSyntax syntax;

        public ReturnStatement(StatementSyntax syntax)
        {
            this.syntax = syntax;
        }

        internal new ReturnStatementSyntax GetWrapped()
        {
            throw new NotImplementedException();
        }

        protected override StatementSyntax GetWrappedImpl() => GetWrapped();
    }
}