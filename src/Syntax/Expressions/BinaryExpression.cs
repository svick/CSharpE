using System;
using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using CSharpSyntaxFactory = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Roslyn = Microsoft.CodeAnalysis;

namespace CSharpE.Syntax
{
    public abstract class BinaryExpression : Expression
    {
        private protected ExpressionSyntax Syntax;

        internal override SyntaxNode Parent { get; set; }

        internal BinaryExpression(ExpressionSyntax syntax, SyntaxNode parent)
        {
            Syntax = syntax;
            Parent = parent;
        }

        internal BinaryExpression(Expression left, Expression right)
        {
            Left = left;
            Right = right;
        }

        ExpressionSyntax GetLeft(ExpressionSyntax expression)
        {
            switch (expression)
            {
                case BinaryExpressionSyntax binary: return binary.Left;
                case AssignmentExpressionSyntax assignment: return assignment.Left;
                default: throw new InvalidOperationException();
            }
        }

        private Expression left;
        public Expression Left
        {
            get
            {
                if (left == null)
                    left = FromRoslyn.Expression(GetLeft(Syntax), this);

                return left;
            }
            set => SetNotNull(ref left, value);
        }

        ExpressionSyntax GetRight(ExpressionSyntax expression)
        {
            switch (expression)
            {
                case BinaryExpressionSyntax binary: return binary.Right;
                case AssignmentExpressionSyntax assignment: return assignment.Right;
                default: throw new InvalidOperationException();
            }
        }

        private Expression right;
        public Expression Right
        {
            get
            {
                if (right == null)
                    right = FromRoslyn.Expression(GetRight(Syntax), this);

                return right;
            }
            set => SetNotNull(ref right, value);
        }
        
        private protected abstract SyntaxKind Kind { get; }

        private protected virtual bool IsAssignment => false;

        private protected sealed override ExpressionSyntax GetWrappedExpression(ref bool? changed)
        {
            GetAndResetChanged(ref changed);

            bool? thisChanged = false;

            var newLeft = left?.GetWrapped(ref thisChanged) ?? GetLeft(Syntax);
            var newRight = right?.GetWrapped(ref thisChanged) ?? GetRight(Syntax);

            if (Syntax == null || thisChanged == true)
            {
                Syntax = IsAssignment 
                    ? (ExpressionSyntax)CSharpSyntaxFactory.AssignmentExpression(Kind, newLeft, newRight)
                    : CSharpSyntaxFactory.BinaryExpression(Kind, newLeft, newRight);

                SetChanged(ref changed);
            }

            return Syntax;
        }

        private protected override void SetSyntaxImpl(Roslyn::SyntaxNode newSyntax) =>
            throw new NotImplementedException();
    }
}