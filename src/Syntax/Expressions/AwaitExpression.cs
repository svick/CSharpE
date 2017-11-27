using System;
using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using CSharpSyntaxFactory = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace CSharpE.Syntax
{
    public class AwaitExpression : Expression
    {
        private AwaitExpressionSyntax syntax;

        internal AwaitExpression(AwaitExpressionSyntax syntax) =>
            this.syntax = syntax ?? throw new ArgumentNullException(nameof(syntax));

        public AwaitExpression(Expression operand) =>
            Operand = operand ?? throw new ArgumentNullException(nameof(operand));

        private Expression operand;
        public Expression Operand
        {
            get
            {
                if (operand == null)
                {
                    operand = FromRoslyn.Expression(syntax.Expression);
                }

                return operand;
            }
            set => operand = value ?? throw new ArgumentNullException(nameof(value));
        }

        internal override ExpressionSyntax GetWrapped(WrapperContext context)
        {
            var newOperand = operand?.GetWrapped(context) ?? syntax.Expression;

            if (syntax == null || newOperand != syntax.Expression)
            {
                syntax = CSharpSyntaxFactory.AwaitExpression(newOperand);
            }

            return syntax;
        }
    }
}