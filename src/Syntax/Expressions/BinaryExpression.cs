using System;
using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using CSharpSyntaxFactory = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Roslyn = Microsoft.CodeAnalysis;

namespace CSharpE.Syntax
{
    public abstract class BinaryExpression : Expression, ISyntaxWrapper<BinaryExpressionSyntax>
    {
        private protected BinaryExpressionSyntax Syntax;

        internal override SyntaxNode Parent { get; set; }

        internal BinaryExpression(BinaryExpressionSyntax syntax, SyntaxNode parent)
        {
            Syntax = syntax;
            Parent = parent;
        }

        internal BinaryExpression(Expression left, Expression right)
        {
            Left = left;
            Right = right;
        }

        private Expression left;
        public Expression Left
        {
            get
            {
                if (left == null)
                    left = FromRoslyn.Expression(Syntax.Left, this);

                return left;
            }
            set => SetNotNull(ref left, value);
        }

        private Expression right;
        public Expression Right
        {
            get
            {
                if (right == null)
                    right = FromRoslyn.Expression(Syntax.Right, this);

                return right;
            }
            set => SetNotNull(ref right, value);
        }
        
        private protected abstract SyntaxKind Kind { get; }

        BinaryExpressionSyntax ISyntaxWrapper<BinaryExpressionSyntax>.GetWrapped(ref bool? changed)
        {
            GetAndResetChanged(ref changed);

            bool? thisChanged = false;

            var newLeft = left?.GetWrapped(ref thisChanged) ?? Syntax.Left;
            var newRight = right?.GetWrapped(ref thisChanged) ?? Syntax.Right;

            if (Syntax == null || thisChanged == true)
            {
                Syntax = CSharpSyntaxFactory.BinaryExpression(Kind, newLeft, newRight);

                SetChanged(ref changed);
            }

            return Syntax;
        }

        private protected sealed override ExpressionSyntax GetWrappedExpression(ref bool? changed) =>
            this.GetWrapped<BinaryExpressionSyntax>(ref changed);

        private protected override void SetSyntaxImpl(Roslyn::SyntaxNode newSyntax) =>
            throw new NotImplementedException();

    }
}