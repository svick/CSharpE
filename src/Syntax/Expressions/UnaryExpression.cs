using System;
using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RoslynSyntaxFactory = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Roslyn = Microsoft.CodeAnalysis;

namespace CSharpE.Syntax
{
    public abstract class UnaryExpression : Expression
    {
        private ExpressionSyntax syntax;

        internal override SyntaxNode Parent { get; set; }

        internal UnaryExpression(ExpressionSyntax syntax, SyntaxNode parent)
        {
            this.syntax = syntax;
            Parent = parent;
        }

        internal UnaryExpression(Expression operand)
        {
            Operand = operand;
        }

        ExpressionSyntax GetOperand(ExpressionSyntax expression)
        {
            switch (expression)
            {
                case PrefixUnaryExpressionSyntax prefix: return prefix.Operand;
                case PostfixUnaryExpressionSyntax postfix: return postfix.Operand;
                default: throw new InvalidOperationException();
            }
        }

        private Expression operand;
        public Expression Operand
        {
            get
            {
                if (operand == null)
                    operand = FromRoslyn.Expression(GetOperand(syntax), this);

                return operand;
            }
            set => SetNotNull(ref operand, value);
        }

        private protected abstract SyntaxKind Kind { get; }

        private protected virtual bool IsPrefix => true;

        private protected sealed override ExpressionSyntax GetWrappedExpression(ref bool? changed)
        {
            GetAndResetChanged(ref changed);

            bool? thisChanged = false;

            var newOperand = operand?.GetWrapped(ref thisChanged) ?? GetOperand(syntax);

            if (syntax == null || thisChanged == true)
            {
                syntax = IsPrefix 
                    ? (ExpressionSyntax)RoslynSyntaxFactory.PrefixUnaryExpression(Kind, newOperand)
                    : RoslynSyntaxFactory.PostfixUnaryExpression(Kind, newOperand);

                SetChanged(ref changed);
            }

            return syntax;
        }

        private protected override void SetSyntaxImpl(Roslyn::SyntaxNode newSyntax)
        {
            Set(ref operand, null);
            syntax = (ExpressionSyntax)newSyntax;
        }
    }
}