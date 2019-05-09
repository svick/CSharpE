using System;
using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RoslynSyntaxFactory = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Roslyn = Microsoft.CodeAnalysis;

namespace CSharpE.Syntax
{
    public abstract class Statement : SyntaxNode, ISyntaxWrapper<StatementSyntax>
    {
        private protected Statement() { }
        private protected Statement(Roslyn::SyntaxNode syntax) : base(syntax) { }

        StatementSyntax ISyntaxWrapper<StatementSyntax>.GetWrapped(ref bool? changed) => GetWrappedStatement(ref changed);

        private protected abstract StatementSyntax GetWrappedStatement(ref bool? changed);

        public abstract void ReplaceExpressions<T>(Func<T, bool> filter, Func<T, Expression> projection) where T : Expression;

        protected static Roslyn::SyntaxList<StatementSyntax> GetStatementList(StatementSyntax statement)
        {
            switch (statement)
            {
                case null:
                    return RoslynSyntaxFactory.List<StatementSyntax>();
                case BlockSyntax blockSyntax:
                    return blockSyntax.Statements;
                default:
                    return RoslynSyntaxFactory.SingletonList(statement);
            }
        }


    }
}