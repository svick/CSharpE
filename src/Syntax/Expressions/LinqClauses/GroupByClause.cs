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
            Init(syntax);
            Parent = parent;
        }

        private void Init(GroupClauseSyntax syntax)
        {
            this.syntax = syntax;
            Into = GetSyntaxInto();
        }

        public GroupByClause(Expression groupExpression, Expression byExpression, string into = null)
        {
            GroupExpression = groupExpression;
            ByExpression = byExpression;
            Into = into;
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

        private string GetSyntaxInto() => LinqExpression.GetSyntaxInto(syntax);
        public string Into { get; set; }

        internal override Roslyn::SyntaxNode GetWrapped(ref bool? changed)
        {
            GetAndResetChanged(ref changed, out var thisChanged);

            var newGroupExpression = groupExpression?.GetWrapped(ref thisChanged) ?? syntax.GroupExpression;
            var newByExpression = byExpression?.GetWrapped(ref thisChanged) ?? syntax.ByExpression;

            if (syntax == null || thisChanged == true || Into != GetSyntaxInto())
            {
                syntax = RoslynSyntaxFactory.GroupClause(newGroupExpression, newByExpression);

                syntax = LinqExpression.WithIntoAnnotation(syntax, Into);

                SetChanged(ref changed);
            }

            return syntax;
        }

        private protected override void SetSyntaxImpl(Roslyn::SyntaxNode newSyntax)
        {
            Init((GroupClauseSyntax)newSyntax);
            Set(ref groupExpression, null);
            Set(ref byExpression, null);
        }

        private protected override SyntaxNode CloneImpl() => new GroupByClause(GroupExpression, ByExpression, Into);

        public override void ReplaceExpressions<T>(Func<T, bool> filter, Func<T, Expression> projection)
        {
            GroupExpression = Expression.ReplaceExpressions(GroupExpression, filter, projection);
            ByExpression = Expression.ReplaceExpressions(ByExpression, filter, projection);
        }
    }
}
