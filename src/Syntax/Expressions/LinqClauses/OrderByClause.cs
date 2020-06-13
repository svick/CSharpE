using System;
using System.Collections.Generic;
using System.Linq;
using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RoslynSyntaxFactory = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Roslyn = Microsoft.CodeAnalysis;

namespace CSharpE.Syntax
{
    public sealed class OrderByClause : LinqClause
    {
        private OrderByClauseSyntax syntax;

        internal OrderByClause(OrderByClauseSyntax syntax, SyntaxNode parent)
            : base(syntax)
        {
            this.syntax = syntax;
            Parent = parent;
        }

        public OrderByClause(Expression expression, bool isDescending = false)
            : this(new Ordering(expression, isDescending)) { }

        public OrderByClause(params Ordering[] orderings) : this(orderings.AsEnumerable()) { }

        public OrderByClause(IEnumerable<Ordering> orderings) =>
            this.orderings = new SeparatedSyntaxList<Ordering, OrderingSyntax>(orderings, this);

        private SeparatedSyntaxList<Ordering, OrderingSyntax> orderings;
        public IList<Ordering> Orderings
        {
            get
            {
                if (orderings == null)
                    orderings = new SeparatedSyntaxList<Ordering, OrderingSyntax>(syntax.Orderings, this);

                return orderings;
            }
            set => SetList(ref orderings, new SeparatedSyntaxList<Ordering, OrderingSyntax>(value, this));
        }

        internal override Roslyn::SyntaxNode GetWrapped(ref bool? changed)
        {
            GetAndResetChanged(ref changed, out var thisChanged);

            var newOrderings = orderings?.GetWrapped(ref thisChanged) ?? syntax.Orderings;

            if (syntax == null || thisChanged == true)
            {
                syntax = RoslynSyntaxFactory.OrderByClause(newOrderings);

                SetChanged(ref changed);
            }

            return syntax;
        }

        private protected override void SetSyntaxImpl(Roslyn::SyntaxNode newSyntax)
        {
            syntax = (OrderByClauseSyntax)newSyntax;
            SetList(ref orderings, null);
        }

        private protected override SyntaxNode CloneImpl() => new OrderByClause(Orderings);

        public override void ReplaceExpressions<T>(Func<T, bool> filter, Func<T, Expression> projection)
        {
            foreach (var ordering in Orderings)
            {
                ordering.ReplaceExpressions(filter, projection);
            }
        }
    }

    public sealed class Ordering : SyntaxNode, ISyntaxWrapper<OrderingSyntax>
    {
        private OrderingSyntax syntax;

        internal Ordering(OrderingSyntax syntax, OrderByClause parent)
            : base(syntax)
        {
            Init(syntax);
            Parent = parent;
        }

        private void Init(OrderingSyntax syntax)
        {
            this.syntax = syntax;
            IsDescending = IsSyntaxDescending();
        }

        public Ordering(Expression expression, bool isDescending = false)
        {
            Expression = expression;
            IsDescending = isDescending;
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

        private bool IsSyntaxDescending() => syntax.Kind() == SyntaxKind.DescendingOrdering;

        public bool IsDescending { get; set; }

        OrderingSyntax ISyntaxWrapper<OrderingSyntax>.GetWrapped(ref bool? changed)
        {
            GetAndResetChanged(ref changed);

            bool? thisChanged = false;

            var newExpression = expression?.GetWrapped(ref thisChanged) ?? syntax.Expression;

            if (syntax == null || thisChanged == true || IsDescending != IsSyntaxDescending())
            {
                var kind = IsDescending ? SyntaxKind.DescendingOrdering : SyntaxKind.AscendingOrdering;
                var descendingKeyword = IsDescending ? RoslynSyntaxFactory.Token(SyntaxKind.DescendingKeyword) : default;

                syntax = RoslynSyntaxFactory.Ordering(kind, newExpression, descendingKeyword);

                SetChanged(ref changed);
            }

            return syntax;
        }

        private protected override void SetSyntaxImpl(Roslyn::SyntaxNode newSyntax)
        {
            Init((OrderingSyntax)newSyntax);
            Set(ref expression, null);
        }

        private protected override SyntaxNode CloneImpl() => new Ordering(Expression, IsDescending);

        public void ReplaceExpressions<T>(Func<T, bool> filter, Func<T, Expression> projection) where T : Expression =>
            Expression = Expression.ReplaceExpressions(Expression, filter, projection);
    }
}
