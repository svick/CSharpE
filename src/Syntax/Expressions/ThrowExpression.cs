using System;
using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using CSharpSyntaxFactory = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Roslyn = Microsoft.CodeAnalysis;

namespace CSharpE.Syntax
{
    public sealed class ThrowExpression : Expression, ISyntaxWrapper<ThrowExpressionSyntax>
    {
        private ThrowExpressionSyntax syntax;
        
        internal ThrowExpression(ThrowExpressionSyntax syntax, SyntaxNode parent)
        {
            this.syntax = syntax;
            Parent = parent;
        }

        public ThrowExpression(Expression operand) =>
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
        
        ThrowExpressionSyntax ISyntaxWrapper<ThrowExpressionSyntax>.GetWrapped(ref bool? changed)
        {
            GetAndResetChanged(ref changed);

            bool? thisChanged = false;

            var newOperand = operand?.GetWrapped(ref thisChanged) ?? syntax.Expression;

            if (syntax == null || thisChanged == true)
            {
                syntax = CSharpSyntaxFactory.ThrowExpression(newOperand);

                SetChanged(ref changed);
            }

            return syntax;
        }

        private protected override ExpressionSyntax GetWrappedExpression(ref bool? changed)
            => this.GetWrapped<ThrowExpressionSyntax>(ref changed);

        private protected override void SetSyntaxImpl(Roslyn::SyntaxNode newSyntax)
        {
            syntax = (ThrowExpressionSyntax)newSyntax;

            Set(ref operand, null);
        }

        internal override SyntaxNode Clone() => new ThrowExpression(Operand);

        internal override SyntaxNode Parent { get; set; }
    }
}