using System;
using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RoslynSyntaxFactory = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Roslyn = Microsoft.CodeAnalysis;

namespace CSharpE.Syntax
{
    public sealed class WhereClause : LinqClause
    {
        private WhereClauseSyntax syntax;

        internal WhereClause(WhereClauseSyntax syntax, SyntaxNode parent)
            : base(syntax)
        {
            this.syntax = syntax;
            Parent = parent;
        }

        public WhereClause(Expression condition) => Condition = condition;

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

        internal override Roslyn::SyntaxNode GetWrapped(ref bool? changed)
        {
            GetAndResetChanged(ref changed, out var thisChanged);

            var newCondition = condition?.GetWrapped(ref thisChanged) ?? syntax.Condition;

            if (syntax == null || thisChanged == true)
            {
                syntax = RoslynSyntaxFactory.WhereClause(newCondition);

                SetChanged(ref changed);
            }

            return syntax;
        }

        private protected override void SetSyntaxImpl(Roslyn::SyntaxNode newSyntax)
        {
            syntax = (WhereClauseSyntax)newSyntax;
            Set(ref condition, null);
        }

        private protected override SyntaxNode CloneImpl() => new WhereClause(Condition);

        public override void ReplaceExpressions<T>(Func<T, bool> filter, Func<T, Expression> projection) =>
            Condition = Expression.ReplaceExpressions(Condition, filter, projection);
    }
}
