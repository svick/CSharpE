using System;
using System.Diagnostics;
using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RoslynSyntaxFactory = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Roslyn = Microsoft.CodeAnalysis;

namespace CSharpE.Syntax
{
    // Roslyn doesn't allow ThrowExpression inside an ExpressionStatement, instead it uses ThrowStatement
    // CSharpE always uses ThrowExpression and doesn't have ThrowStatement
    // so this class has to translate between the two
    public sealed class ExpressionStatement : Statement
    {
        private StatementSyntax syntax;

        internal ExpressionStatement(StatementSyntax syntax, SyntaxNode parent)
            : base(syntax)
        {
            Debug.Assert(syntax is ExpressionStatementSyntax || syntax is ThrowStatementSyntax);

            this.syntax = syntax;
            Parent = parent;
        }

        public ExpressionStatement(Expression expression) => Expression = expression;

        private Expression expression;
        public Expression Expression
        {
            get
            {
                if (expression == null)
                {
                    if (syntax is ThrowStatementSyntax throwStatement)
                    {
                        // HACK: ThrowExpression ctor with an expression from Roslyn syntax
                        // requires that expression to be already part of the tree
                        expression = FromRoslyn.Expression(throwStatement.Expression, this);
                        Expression = new ThrowExpression(expression);
                    }
                    else
                    {
                        expression = FromRoslyn.Expression(((ExpressionStatementSyntax)syntax).Expression, this);
                    }
                }

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

            if (syntax == null || newIsThrow != oldIsThrow || thisChanged == true || ShouldAnnotate(syntax, changed))
            {
                if (newIsThrow)
                    syntax = RoslynSyntaxFactory.ThrowStatement(newExpression);
                else
                    syntax = RoslynSyntaxFactory.ExpressionStatement(newExpression);

                syntax = Annotate(syntax);

                SetChanged(ref changed);
            }

            return syntax;
        }

        private protected override void SetSyntaxImpl(Roslyn::SyntaxNode newSyntax)
        {
            syntax = (StatementSyntax)newSyntax;

            Set(ref expression, null);
        }

        private protected override SyntaxNode CloneImpl() => new ExpressionStatement(Expression);

        public override void ReplaceExpressions<T>(Func<T, bool> filter, Func<T, Expression> projection) =>
            Expression = Expression.ReplaceExpressions(Expression, filter, projection);
    }
}