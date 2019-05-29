using System;
using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RoslynSyntaxFactory = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Roslyn = Microsoft.CodeAnalysis;

namespace CSharpE.Syntax
{
    public sealed class GroupByClause : LinqClause
    {
        private GroupClauseSyntax syntax;

        internal GroupByClause(GroupClauseSyntax syntax, SyntaxNode parent)
            : base(syntax)
        {
            this.syntax = syntax;
            Parent = parent;
        }

        public GroupByClause(Expression groupExpression, Expression byExpression)
        {
            GroupExpression = groupExpression;
            ByExpression = byExpression;
        }

        private Expression groupExpression;
        public Expression GroupExpression
        {
            get
            {
                if (groupExpression == null)
                    groupExpression = FromRoslyn.Expression(syntax.GroupExpression, this);

                return groupExpression;
            }
            set => SetNotNull(ref groupExpression, value);
        }

        private Expression byExpression;
        public Expression ByExpression
        {
            get
            {
                if (byExpression == null)
                    byExpression = FromRoslyn.Expression(syntax.ByExpression, this);

                return byExpression;
            }
            set => SetNotNull(ref byExpression, value);
        }

        internal override Roslyn::SyntaxNode GetWrapped(ref bool? changed)
        {
            GetAndResetChanged(ref changed, out var thisChanged);

            var newGroupExpression = groupExpression?.GetWrapped(ref thisChanged) ?? syntax.GroupExpression;
            var newByExpression = byExpression?.GetWrapped(ref thisChanged) ?? syntax.ByExpression;

            if (syntax == null || thisChanged == true)
            {
                syntax = RoslynSyntaxFactory.GroupClause(newGroupExpression, newByExpression);

                SetChanged(ref changed);
            }

            return syntax;
        }

        private protected override void SetSyntaxImpl(Roslyn::SyntaxNode newSyntax)
        {
            syntax = (GroupClauseSyntax)newSyntax;
            Set(ref groupExpression, null);
            Set(ref byExpression, null);
        }

        private protected override SyntaxNode CloneImpl() => new GroupByClause(GroupExpression, ByExpression);

        public override void ReplaceExpressions<T>(Func<T, bool> filter, Func<T, Expression> projection)
        {
            GroupExpression = Expression.ReplaceExpressions(GroupExpression, filter, projection);
            ByExpression = Expression.ReplaceExpressions(ByExpression, filter, projection);
        }
    }
}
