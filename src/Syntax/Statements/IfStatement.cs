using System.Collections.Generic;
using System.Linq;
using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RoslynSyntaxFactory = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Roslyn = Microsoft.CodeAnalysis;

namespace CSharpE.Syntax
{
    public sealed class IfStatement : Statement, ISyntaxWrapper<IfStatementSyntax>
    {
        private IfStatementSyntax syntax;

        internal IfStatement(IfStatementSyntax syntax, SyntaxNode parent)
        {
            this.syntax = syntax;
            Parent = parent;
        }

        public IfStatement(Expression condition, IEnumerable<Statement> thenStatements)
        {
            Condition = condition;
            ThenStatements = thenStatements.ToList();
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

        private static Roslyn::SyntaxList<StatementSyntax> GetStatementList(StatementSyntax statement) =>
            statement is BlockSyntax blockSyntax
                ? blockSyntax.Statements
                : RoslynSyntaxFactory.SingletonList(statement);

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

        // TODO: else

        IfStatementSyntax ISyntaxWrapper<IfStatementSyntax>.GetWrapped(ref bool? changed)
        {
            GetAndResetChanged(ref changed);

            bool? thisChanged = false;

            var newCondition = condition?.GetWrapped(ref thisChanged) ?? syntax.Condition;
            var newThenStatements = thenStatements?.GetWrapped(ref thisChanged) ?? GetStatementList(syntax.Statement);

            if (syntax == null || thisChanged == true)
            {
                syntax = RoslynSyntaxFactory.IfStatement(newCondition, RoslynSyntaxFactory.Block(newThenStatements));

                SetChanged(ref changed);
            }

            return syntax;
        }

        private protected override StatementSyntax GetWrappedStatement(ref bool? changed) =>
            this.GetWrapped<IfStatementSyntax>(ref changed);

        private protected override void SetSyntaxImpl(Roslyn::SyntaxNode newSyntax)
        {
            syntax = (IfStatementSyntax)newSyntax;

            Set(ref condition, null);
            thenStatements = null;
        }

        internal override SyntaxNode Clone() => new IfStatement(Condition, ThenStatements);

        internal override SyntaxNode Parent { get; set; }
    }
}