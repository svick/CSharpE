using System;
using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RoslynSyntaxFactory = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Roslyn = Microsoft.CodeAnalysis;

namespace CSharpE.Syntax
{
    public sealed class RefExpression : Expression
    {
        private RefExpressionSyntax syntax;
        
        internal RefExpression(RefExpressionSyntax syntax, SyntaxNode parent)
            : base(syntax)
        {
            this.syntax = syntax;
            Parent = parent;
        }

        public RefExpression(Expression expression) =>
            Expression = expression ?? throw new ArgumentNullException(nameof(expression));

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
            GetAndResetChanged(ref changed, out var thisChanged);

            var newExpression = expression?.GetWrapped(ref thisChanged) ?? syntax.Expression;

            if (syntax == null || thisChanged == true || ShouldAnnotate(syntax, changed))
            {
                syntax = RoslynSyntaxFactory.RefExpression(newExpression);

                syntax = Annotate(syntax);

                SetChanged(ref changed);
            }

            return syntax;
        }

        private protected override void SetSyntaxImpl(Roslyn::SyntaxNode newSyntax)
        {
            Set(ref expression, null);
            syntax = (RefExpressionSyntax)newSyntax;
        }

        private protected override SyntaxNode CloneImpl() => new RefExpression(Expression);
    }
}