using System;
using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using CSharpSyntaxFactory = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Roslyn = Microsoft.CodeAnalysis;

namespace CSharpE.Syntax
{
    public sealed class ExpressionStatement : Statement
    {
        private ExpressionStatementSyntax syntax;

        internal ExpressionStatement(ExpressionStatementSyntax syntax, SyntaxNode parent)
        {
            this.syntax = syntax ?? throw new ArgumentNullException(nameof(syntax));
            Parent = parent;
        }

        public ExpressionStatement(Expression expression) =>
            Expression = expression ?? throw new ArgumentNullException(nameof(expression));

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

        internal ExpressionStatementSyntax GetWrapped(ref bool? changed)
        {
            GetAndResetChanged(ref changed);

            bool? thisChanged = false;

            var newExpression = expression?.GetWrapped(ref thisChanged) ?? syntax.Expression;

            if (syntax == null || thisChanged == true)
            {
                syntax = CSharpSyntaxFactory.ExpressionStatement(newExpression);

                SetChanged(ref changed);
            }

            return syntax;
        }

        protected override StatementSyntax GetWrappedStatement(ref bool? changed) => GetWrapped(ref changed);

        private protected override void SetSyntaxImpl(Roslyn::SyntaxNode newSyntax)
        {
            syntax = (ExpressionStatementSyntax)newSyntax;

            Set(ref expression, null);
        }

        internal override SyntaxNode Clone() => new ExpressionStatement(Expression);

        internal override SyntaxNode Parent { get; set; }
    }
}