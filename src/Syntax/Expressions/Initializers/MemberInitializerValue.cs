using System;
using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Roslyn = Microsoft.CodeAnalysis;

namespace CSharpE.Syntax
{
    public abstract class MemberInitializerValue : SyntaxNode, ISyntaxWrapper<ExpressionSyntax>
    {
        private protected MemberInitializerValue() { }
        private protected MemberInitializerValue(ExpressionSyntax syntax) : base(syntax) { }

        internal abstract ExpressionSyntax GetWrapped(ref bool? changed);

        ExpressionSyntax ISyntaxWrapper<ExpressionSyntax>.GetWrapped(ref bool? changed)
            => GetWrapped(ref changed);

        public abstract void ReplaceExpressions<T>(Func<T, bool> filter, Func<T, Expression> projection)
            where T : Expression;
    }

    public sealed class ExpressionMemberInitializerValue : MemberInitializerValue
    {
        private ExpressionSyntax syntax;

        internal ExpressionMemberInitializerValue(ExpressionSyntax syntax, MemberInitializer parent)
            : base(syntax)
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
            GetAndResetChanged(ref changed, out var thisChanged);

            var newExpression = expression?.GetWrapped(ref thisChanged) ?? syntax;

            if (syntax == null || thisChanged == true)
            {
                syntax = newExpression;

                SetChanged(ref changed);
            }

            return syntax;
        }

        private protected override void SetSyntaxImpl(Roslyn::SyntaxNode newSyntax)
        {
            Set(ref expression, null);
            syntax = (ExpressionSyntax)newSyntax;
        }

        private protected override SyntaxNode CloneImpl() => new ExpressionMemberInitializerValue(Expression);

        public override void ReplaceExpressions<T>(Func<T, bool> filter, Func<T, Expression> projection) =>
            Expression = Expression.ReplaceExpressions(Expression, filter, projection);
    }

    public sealed class InitializerMemberInitializerValue : MemberInitializerValue
    {
        private InitializerExpressionSyntax syntax;

        internal InitializerMemberInitializerValue(InitializerExpressionSyntax syntax, MemberInitializer parent)
            : base(syntax)
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
            GetAndResetChanged(ref changed, out var thisChanged);

            var newInitializer = initializer?.GetWrapped(ref thisChanged) ?? syntax;

            if (syntax == null || thisChanged == true)
            {
                syntax = newInitializer;

                SetChanged(ref changed);
            }

            return syntax;
        }

        private protected override void SetSyntaxImpl(Roslyn::SyntaxNode newSyntax)
        {
            Set(ref initializer, null);
            syntax = (InitializerExpressionSyntax)newSyntax;
        }

        private protected override SyntaxNode CloneImpl() => new InitializerMemberInitializerValue(Initializer);

        public override void ReplaceExpressions<T>(Func<T, bool> filter, Func<T, Expression> projection) =>
            Initializer.ReplaceExpressions(filter, projection);
    }
}
