using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RoslynSyntaxFactory = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Roslyn = Microsoft.CodeAnalysis;

namespace CSharpE.Syntax
{
    public sealed class IsPatternExpression : Expression
    {
        private IsPatternExpressionSyntax syntax;

        internal IsPatternExpression(IsPatternExpressionSyntax syntax, SyntaxNode parent)
            : base(syntax)
        {
            this.syntax = syntax;
            Parent = parent;
        }

        public IsPatternExpression(Expression expression, Pattern pattern)
        {
            Expression = expression;
            Pattern = pattern;
        }

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

        private Pattern pattern;
        public Pattern Pattern
        {
            get
            {
                if (pattern == null)
                    pattern = FromRoslyn.Pattern(syntax.Pattern, this);

                return pattern;
            }
            set => SetNotNull(ref pattern, value);
        }

        private protected override ExpressionSyntax GetWrappedExpression(ref bool? changed)
        {
            GetAndResetChanged(ref changed);

            bool? thisChanged = false;

            var newExpression = expression?.GetWrapped(ref thisChanged) ?? syntax.Expression;
            var newPattern = pattern?.GetWrapped(ref thisChanged) ?? syntax.Pattern;

            if (syntax == null || thisChanged == true || ShouldAnnotate(syntax, changed))
            {
                syntax = RoslynSyntaxFactory.IsPatternExpression(newExpression, newPattern);

                syntax = Annotate(syntax);

                SetChanged(ref changed);
            }

            return syntax;
        }

        private protected override void SetSyntaxImpl(Roslyn::SyntaxNode newSyntax)
        {
            syntax = (IsPatternExpressionSyntax)newSyntax;
            Set(ref expression, null);
            Set(ref pattern, null);
        }

        private protected override SyntaxNode CloneImpl() => new IsPatternExpression(Expression, Pattern);
    }
}