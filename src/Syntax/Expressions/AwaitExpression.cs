using System;
using System.Collections.Generic;
using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using CSharpSyntaxFactory = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace CSharpE.Syntax
{
    public sealed class AwaitExpression : Expression
    {
        private AwaitExpressionSyntax syntax;

        internal AwaitExpression(AwaitExpressionSyntax syntax, SyntaxNode parent)
        {
            this.syntax = syntax ?? throw new ArgumentNullException(nameof(syntax));
            Parent = parent;
        }

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

        internal override ExpressionSyntax GetWrapped()
        {
            var newOperand = operand?.GetWrapped() ?? syntax.Expression;

            if (syntax == null || newOperand != syntax.Expression)
            {
                syntax = CSharpSyntaxFactory.AwaitExpression(newOperand);
            }

            return syntax;
        }

        protected override IEnumerable<IEnumerable<SyntaxNode>> GetChildren()
        {
            yield return Node(Operand);
        }

        internal override SyntaxNode Parent { get; set; }
    }
}