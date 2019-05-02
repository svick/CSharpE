using System;
using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RoslynSyntaxFactory = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Roslyn = Microsoft.CodeAnalysis;

namespace CSharpE.Syntax
{
    public abstract class BinaryExpression : Expression
    {
        private ExpressionSyntax syntax;

        internal BinaryExpression(ExpressionSyntax syntax, SyntaxNode parent)
        {
            this.syntax = syntax;
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
                    left = FromRoslyn.Expression(GetLeft(syntax), this);

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
                    right = FromRoslyn.Expression(GetRight(syntax), this);

                return right;
            }
            set => SetNotNull(ref right, value);
        }

        // PERF: consider caching the parsed value?
        private protected virtual SyntaxKind Kind =>
            Enum.TryParse(GetType().Name, out SyntaxKind kind) ? kind : throw new InvalidOperationException();

        private protected virtual bool IsAssignment => false;

        private protected sealed override ExpressionSyntax GetWrappedExpression(ref bool? changed)
        {
            GetAndResetChanged(ref changed);

            bool? thisChanged = false;

            var newLeft = left?.GetWrapped(ref thisChanged) ?? GetLeft(syntax);
            var newRight = right?.GetWrapped(ref thisChanged) ?? GetRight(syntax);

            if (syntax == null || thisChanged == true)
            {
                syntax = IsAssignment 
                    ? (ExpressionSyntax)RoslynSyntaxFactory.AssignmentExpression(Kind, newLeft, newRight)
                    : RoslynSyntaxFactory.BinaryExpression(Kind, newLeft, newRight);

                SetChanged(ref changed);
            }

            return syntax;
        }

        private protected override void SetSyntaxImpl(Roslyn::SyntaxNode newSyntax)
        {
            Set(ref left, null);
            Set(ref right, null);
            syntax = (ExpressionSyntax)newSyntax;
        }
    }
}