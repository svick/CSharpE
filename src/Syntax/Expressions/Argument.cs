using System;
using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using CSharpSyntaxFactory = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Roslyn = Microsoft.CodeAnalysis;

namespace CSharpE.Syntax
{
    // TODO: named arguments; ref and out
    public sealed class Argument : SyntaxNode, ISyntaxWrapper2<ArgumentSyntax>
    {
        private ArgumentSyntax syntax;

        internal Argument(ArgumentSyntax syntax, SyntaxNode parent)
        {
            this.syntax = syntax ?? throw new ArgumentNullException(nameof(syntax));
            Parent = parent;
        }

        public Argument(Expression expression) => Expression = expression;

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

        internal ArgumentSyntax GetWrapped(ref bool changed)
        {
            changed |= GetAndResetSyntaxSet();

            bool thisChanged = false;

            var newExpression = expression?.GetWrapped(ref thisChanged) ?? syntax.Expression;

            if (syntax == null || thisChanged)
            {
                syntax = CSharpSyntaxFactory.Argument(newExpression);

                changed = true;
            }

            return syntax;
        }

        ArgumentSyntax ISyntaxWrapper2<ArgumentSyntax>.GetWrapped(ref bool changed) => GetWrapped(ref changed);

        public static implicit operator Argument(Expression expression) => new Argument(expression);

        protected override void SetSyntaxImpl(Roslyn::SyntaxNode newSyntax)
        {
            syntax = (ArgumentSyntax)newSyntax;
            expression = null;
        }

        internal override SyntaxNode Clone() => new Argument(Expression);

        internal override SyntaxNode Parent { get; set; }
    }
}