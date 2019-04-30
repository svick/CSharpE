using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RoslynSyntaxFactory = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Roslyn = Microsoft.CodeAnalysis;

namespace CSharpE.Syntax
{
    public sealed class ConditionalExpression : Expression
    {
        private ConditionalExpressionSyntax syntax;

        internal ConditionalExpression(ConditionalExpressionSyntax syntax, SyntaxNode parent)
        {
            this.syntax = syntax;
            Parent = parent;
        }

        public ConditionalExpression(Expression condition, Expression whenTrue, Expression whenFalse)
        {
            Condition = condition;
            WhenTrue = whenTrue;
            WhenFalse = whenFalse;
        }

        private Expression condition;
        public Expression Condition
        {
            get
            {
                if (condition == null)
                    condition = FromRoslyn.Expression(syntax.Condition, this);

                return condition;
            }
            set => SetNotNull(ref condition, value);
        }

        private Expression whenTrue;
        public Expression WhenTrue
        {
            get
            {
                if (whenTrue == null)
                    whenTrue = FromRoslyn.Expression(syntax.WhenTrue, this);

                return whenTrue;
            }
            set => SetNotNull(ref whenTrue, value);
        }

        private Expression whenFalse;
        public Expression WhenFalse
        {
            get
            {
                if (whenFalse == null)
                    whenFalse = FromRoslyn.Expression(syntax.WhenFalse, this);

                return whenFalse;
            }
            set => SetNotNull(ref whenFalse, value);
        }

        private protected override ExpressionSyntax GetWrappedExpression(ref bool? changed)
        {
            GetAndResetChanged(ref changed);

            bool? thisChanged = false;

            var newCondition = condition?.GetWrapped(ref thisChanged) ?? syntax.Condition;
            var newWhenTrue = whenTrue?.GetWrapped(ref thisChanged) ?? syntax.WhenTrue;
            var newWhenFalse = whenFalse?.GetWrapped(ref thisChanged) ?? syntax.WhenFalse;

            if (syntax == null || thisChanged == true)
            {
                syntax = RoslynSyntaxFactory.ConditionalExpression(newCondition, newWhenTrue, newWhenFalse);

                SetChanged(ref changed);
            }

            return syntax;
        }

        private protected override void SetSyntaxImpl(Roslyn::SyntaxNode newSyntax)
        {
            syntax = (ConditionalExpressionSyntax)newSyntax;

            Set(ref condition, null);
            Set(ref whenTrue, null);
            Set(ref whenFalse, null);
        }

        private protected override SyntaxNode CloneImpl() => new ConditionalExpression(Condition, WhenTrue, WhenFalse);

        internal override SyntaxNode Parent { get; set; }
    }
}