using System;
using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RoslynSyntaxFactory = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Roslyn = Microsoft.CodeAnalysis;

namespace CSharpE.Syntax
{
    public sealed class CheckedExpression : Expression
    {
        private CheckedExpressionSyntax syntax;
        
        internal CheckedExpression(CheckedExpressionSyntax syntax, SyntaxNode parent)
        {
            Init(syntax);
            Parent = parent;
        }

        private void Init(CheckedExpressionSyntax syntax)
        {
            this.syntax = syntax;
            IsChecked = syntax.Kind() == SyntaxKind.CheckedExpression;
        }

        public CheckedExpression(bool isChecked, Expression expression)
        {
            IsChecked = isChecked;
            Expression = expression ?? throw new ArgumentNullException(nameof(expression));
        }

        public bool IsChecked { get; set; }

        private Expression expression;
        public Expression Expression
        {
            get
            {
                if (expression == null)
                    expression = FromRoslyn.Expression(syntax.Expression, this);

                return expression;
            }
            set => SetNotNull(ref expression, value);
        }

        private protected override ExpressionSyntax GetWrappedExpression(ref bool? changed)
        {
            GetAndResetChanged(ref changed);

            bool? thisChanged = false;

            var newExpression = expression?.GetWrapped(ref thisChanged) ?? syntax.Expression;

            bool oldIsChecked = syntax.Kind() == SyntaxKind.CheckedExpression;

            if (syntax == null || thisChanged == true || oldIsChecked != IsChecked || ShouldAnnotate(syntax, changed))
            {
                syntax = RoslynSyntaxFactory.CheckedExpression(
                    IsChecked ? SyntaxKind.CheckedExpression : SyntaxKind.UncheckedExpression, newExpression);

                syntax = Annotate(syntax);

                SetChanged(ref changed);
            }

            return syntax;
        }

        private protected override void SetSyntaxImpl(Roslyn::SyntaxNode newSyntax)
        {
            Set(ref expression, null);
            Init((CheckedExpressionSyntax)newSyntax);
        }

        private protected override SyntaxNode CloneImpl() => new CheckedExpression(IsChecked, Expression);
    }
}