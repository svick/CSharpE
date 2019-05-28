using System;
using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RoslynSyntaxFactory = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Roslyn = Microsoft.CodeAnalysis;

namespace CSharpE.Syntax
{
    public sealed class SelectClause : LinqClause
    {
        private SelectClauseSyntax syntax;

        internal SelectClause(SelectClauseSyntax syntax, SyntaxNode parent)
            : base(syntax)
        {
            this.syntax = syntax;
            Parent = parent;
        }

        public SelectClause(Expression expression) => Expression = expression;

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

        internal override Roslyn::SyntaxNode GetWrapped(ref bool? changed)
        {
            GetAndResetChanged(ref changed, out var thisChanged);

            var newExpression = expression?.GetWrapped(ref thisChanged) ?? syntax.Expression;

            if (syntax == null || thisChanged == true)
            {
                syntax = RoslynSyntaxFactory.SelectClause(newExpression);

                SetChanged(ref changed);
            }

            return syntax;
        }

        private protected override void SetSyntaxImpl(Roslyn::SyntaxNode newSyntax)
        {
            this.syntax = (SelectClauseSyntax)newSyntax;
            Set(ref expression, null);
        }

        private protected override SyntaxNode CloneImpl() => new SelectClause(Expression);

        public override void ReplaceExpressions<T>(Func<T, bool> filter, Func<T, Expression> projection) =>
            Expression = Expression.ReplaceExpressions(Expression, filter, projection);
    }
}
