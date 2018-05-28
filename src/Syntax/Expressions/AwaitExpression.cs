using System;
using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using CSharpSyntaxFactory = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Roslyn = Microsoft.CodeAnalysis;

namespace CSharpE.Syntax
{
    public sealed class AwaitExpression : Expression
    {
        private AwaitExpressionSyntax syntax;

        internal AwaitExpression(AwaitExpressionSyntax syntax, SyntaxNode parent)
        {
            this.syntax = syntax;
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
                    operand = FromRoslyn.Expression(syntax.Expression, this);

                return operand;
            }
            set => SetNotNull(ref operand, value);
        }

        internal override ExpressionSyntax GetWrapped(ref bool? changed)
        {
            GetAndResetChanged(ref changed);

            bool? thisChanged = false;

            var newOperand = operand?.GetWrapped(ref thisChanged) ?? syntax.Expression;

            if (syntax == null || thisChanged == true)
            {
                syntax = CSharpSyntaxFactory.AwaitExpression(newOperand);

                SetChanged(ref changed);
            }

            return syntax;
        }

        protected override void SetSyntaxImpl(Roslyn::SyntaxNode newSyntax)
        {
            syntax = (AwaitExpressionSyntax)newSyntax;

            Set(ref operand, null);
        }

        internal override SyntaxNode Clone() => new AwaitExpression(Operand);

        internal override SyntaxNode Parent { get; set; }
    }
}