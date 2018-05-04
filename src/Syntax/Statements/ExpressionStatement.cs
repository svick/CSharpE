using System;
using System.Collections.Generic;
using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using CSharpSyntaxFactory = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

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
                    expression = FromRoslyn.Expression(syntax.Expression);

                return expression;
            }
            set => expression = value ?? throw new ArgumentNullException(nameof(value));
        }

        internal new ExpressionStatementSyntax GetWrapped()
        {
            var newExpression = expression?.GetWrapped() ?? syntax.Expression;

            if (syntax == null || newExpression != syntax.Expression)
            {
                syntax = CSharpSyntaxFactory.ExpressionStatement(newExpression);
            }

            return syntax;
        }

        protected override StatementSyntax GetWrappedImpl() => GetWrapped();

        protected override IEnumerable<IEnumerable<SyntaxNode>> GetChildren()
        {
            yield return Node(Expression);
        }

        public override SyntaxNode Parent { get; internal set; }
    }
}