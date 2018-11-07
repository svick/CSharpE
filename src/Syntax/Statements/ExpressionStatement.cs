using System;
using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using CSharpSyntaxFactory = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Roslyn = Microsoft.CodeAnalysis;

namespace CSharpE.Syntax
{
    // Roslyn doesn't allow ThrowExpression inside an ExpressionStatement, instead it uses ThrowStatement
    // CSharpE always uses ThrowExpression and doesn't have ThrowStatement
    // so this class has to translate between the two
    public sealed class ExpressionStatement : Statement
    {
        private StatementSyntax syntax;

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
                    expression = syntax is ThrowStatementSyntax throwStatement
                        ? SyntaxFactory.Throw(FromRoslyn.Expression(throwStatement.Expression, this))
                        : FromRoslyn.Expression(((ExpressionStatementSyntax)syntax).Expression, this);

                return expression;
            }
            set => SetNotNull(ref expression, value);
        }

        private protected override StatementSyntax GetWrappedStatement(ref bool? changed)
        {
            GetAndResetChanged(ref changed);

            bool? thisChanged = false;

            ExpressionSyntax newExpression;
            bool newIsThrow = false;

            if (expression is ThrowExpression throwExpression)
            {
                newExpression = throwExpression.Operand.GetWrapped(ref thisChanged);
                newIsThrow = true;
            }
            else if (expression == null && syntax is ThrowStatementSyntax throwStatement)
            {
                newExpression = throwStatement.Expression;
                newIsThrow = true;
            }
            else
            {
                newExpression = expression?.GetWrapped(ref thisChanged) ?? ((ExpressionStatementSyntax)syntax).Expression;
            }

            bool oldIsThrow = syntax is ThrowStatementSyntax;

            if (syntax == null || newIsThrow != oldIsThrow || thisChanged == true)
            {
                if (newIsThrow)
                    syntax = CSharpSyntaxFactory.ThrowStatement(newExpression);
                else
                    syntax = CSharpSyntaxFactory.ExpressionStatement(newExpression);

                SetChanged(ref changed);
            }

            return syntax;
        }

        private protected override void SetSyntaxImpl(Roslyn::SyntaxNode newSyntax)
        {
            syntax = (StatementSyntax)newSyntax;

            Set(ref expression, null);
        }

        internal override SyntaxNode Clone() => new ExpressionStatement(Expression);

        internal override SyntaxNode Parent { get; set; }
    }
}