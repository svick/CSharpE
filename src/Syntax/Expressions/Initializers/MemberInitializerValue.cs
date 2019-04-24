using System;
using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Roslyn = Microsoft.CodeAnalysis;

namespace CSharpE.Syntax
{
    public abstract class MemberInitializerValue : SyntaxNode, ISyntaxWrapper<ExpressionSyntax>
    {
        internal abstract ExpressionSyntax GetWrapped(ref bool? changed);

        ExpressionSyntax ISyntaxWrapper<ExpressionSyntax>.GetWrapped(ref bool? changed)
            => GetWrapped(ref changed);

        public abstract void ReplaceExpressions<T>(Func<T, bool> filter, Func<T, Expression> projection)
            where T : Expression;
    }

    public sealed class ExpressionMemberInitializerValue : MemberInitializerValue
    {
        private ExpressionSyntax syntax;

        public ExpressionMemberInitializerValue(ExpressionSyntax syntax, MemberInitializer parent)
        {
            this.syntax = syntax;
            Parent = parent;
        }

        public ExpressionMemberInitializerValue(Expression expression) => Expression = expression;

        private Expression expression;
        public Expression Expression
        {
            get
            {
                if (expression == null)
                    expression = FromRoslyn.Expression(syntax, this);

                return expression;
            }
            set => SetNotNull(ref expression, value);
        }

        internal override ExpressionSyntax GetWrapped(ref bool? changed)
        {
            GetAndResetChanged(ref changed);

            bool? thisChanged = false;

            var newExpression = expression?.GetWrapped(ref thisChanged) ?? syntax;

            if (syntax == null || thisChanged == true)
            {
                syntax = newExpression;

                SetChanged(ref changed);
            }

            return syntax;
        }

        internal override SyntaxNode Parent { get; set; }

        private protected override void SetSyntaxImpl(Roslyn::SyntaxNode newSyntax)
        {
            Set(ref expression, null);
            syntax = (ExpressionSyntax)newSyntax;
        }

        internal override SyntaxNode Clone() => new ExpressionMemberInitializerValue(Expression);

        public override void ReplaceExpressions<T>(Func<T, bool> filter, Func<T, Expression> projection) =>
            Expression = Expression.ReplaceExpressions(Expression, filter, projection);
    }

    public sealed class InitializerMemberInitializerValue : MemberInitializerValue
    {
        private InitializerExpressionSyntax syntax;

        public InitializerMemberInitializerValue(InitializerExpressionSyntax syntax, MemberInitializer parent)
        {
            this.syntax = syntax;
            Parent = parent;
        }

        public InitializerMemberInitializerValue(Initializer initializer) => Initializer = initializer;

        private Initializer initializer;
        public Initializer Initializer
        {
            get
            {
                if (initializer == null)
                    initializer = FromRoslyn.Initializer(syntax, this);

                return initializer;
            }
            set => SetNotNull(ref initializer, value);
        }

        internal override ExpressionSyntax GetWrapped(ref bool? changed)
        {
            GetAndResetChanged(ref changed);

            bool? thisChanged = false;

            var newInitializer = initializer?.GetWrapped(ref thisChanged) ?? syntax;

            if (syntax == null || thisChanged == true)
            {
                syntax = newInitializer;

                SetChanged(ref changed);
            }

            return syntax;
        }

        internal override SyntaxNode Parent { get; set; }

        private protected override void SetSyntaxImpl(Roslyn::SyntaxNode newSyntax)
        {
            Set(ref initializer, null);
            syntax = (InitializerExpressionSyntax)newSyntax;
        }

        internal override SyntaxNode Clone() => new InitializerMemberInitializerValue(Initializer);

        public override void ReplaceExpressions<T>(Func<T, bool> filter, Func<T, Expression> projection) =>
            Initializer.ReplaceExpressions(filter, projection);
    }
}
