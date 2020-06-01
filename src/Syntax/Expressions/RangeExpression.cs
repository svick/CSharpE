using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RoslynSyntaxFactory = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Roslyn = Microsoft.CodeAnalysis;

namespace CSharpE.Syntax
{
    class RangeExpression : Expression
    {
        private RangeExpressionSyntax syntax;

        internal RangeExpression(RangeExpressionSyntax syntax, SyntaxNode parent)
            : base(syntax)
        {
            this.syntax = syntax;
            Parent = parent;
        }

        public RangeExpression(Expression left, Expression right)
        {
            Left = left;
            Right = right;
        }

        private bool leftSet;
        private Expression left;
        public Expression Left
        {
            get
            {
                if (!leftSet)
                {
                    left = FromRoslyn.Expression(syntax.LeftOperand, this);
                    leftSet = true;
                }

                return left;
            }
            set
            {
                Set(ref left, value);
                leftSet = true;
            }
        }

        private bool rightSet;
        private Expression right;
        public Expression Right
        {
            get
            {
                if (!rightSet)
                {
                    right = FromRoslyn.Expression(syntax.RightOperand, this);
                    rightSet = true;
                }

                return right;
            }
            set
            {
                Set(ref right, value);
                rightSet = true;
            }
        }

        private protected override ExpressionSyntax GetWrappedExpression(ref bool? changed)
        {
            GetAndResetChanged(ref changed, out var thisChanged);

            var newLeft = leftSet ? left?.GetWrapped(ref thisChanged) : syntax.LeftOperand;
            var newRight = rightSet ? right?.GetWrapped(ref thisChanged) : syntax.RightOperand;

            if (syntax == null || thisChanged == true || ShouldAnnotate(syntax, changed))
            {
                syntax = RoslynSyntaxFactory.RangeExpression(newLeft, newRight);
                syntax = Annotate(syntax);

                SetChanged(ref changed);
            }

            return syntax;
        }

        private protected override void SetSyntaxImpl(Roslyn::SyntaxNode newSyntax)
        {
            syntax = (RangeExpressionSyntax)newSyntax;

            Set(ref left, null);
            leftSet = false;
            Set(ref right, null);
            rightSet = false;
        }

        private protected override SyntaxNode CloneImpl() => new RangeExpression(Left, Right);
    }
}
