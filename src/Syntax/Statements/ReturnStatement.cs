using System;
using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using CSharpSyntaxFactory = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace CSharpE.Syntax
{
    public class ReturnStatement : Statement
    {
        private ReturnStatementSyntax syntax;

        internal ReturnStatement(ReturnStatementSyntax syntax) =>
            this.syntax = syntax ?? throw new ArgumentNullException(nameof(syntax));

        public ReturnStatement(Expression expression) => Expression = expression;

        public ReturnStatement() : this((Expression)null) { }

        private bool expressionSet;
        private Expression expression;
        public Expression Expression
        {
            get
            {
                if (!expressionSet)
                {
                    expression = FromRoslyn.Expression(syntax.Expression);
                    expressionSet = true;
                }

                return expression;
            }
            set
            {
                expression = value;
                expressionSet = true;
            }
        }


        internal new ReturnStatementSyntax GetWrapped(WrapperContext context)
        {
            var newExpression = expressionSet ? expression?.GetWrapped(context) : syntax.Expression;

            if (syntax == null || newExpression != syntax.Expression)
            {
                syntax = CSharpSyntaxFactory.ReturnStatement(newExpression);
            }

            return syntax;
        }

        protected override StatementSyntax GetWrappedImpl(WrapperContext context) => GetWrapped(context);
    }
}