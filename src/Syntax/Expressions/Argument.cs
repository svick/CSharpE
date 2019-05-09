using System;
using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RoslynSyntaxFactory = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Roslyn = Microsoft.CodeAnalysis;

namespace CSharpE.Syntax
{
    // TODO: ref, out and in
    public sealed class Argument : SyntaxNode, ISyntaxWrapper<ArgumentSyntax>
    {
        private ArgumentSyntax syntax;

        internal Argument(ArgumentSyntax syntax, SyntaxNode parent)
            : base(syntax)
        {
            this.syntax = syntax ?? throw new ArgumentNullException(nameof(syntax));
            Name = syntax.NameColon?.Name.Identifier.ValueText;
            Parent = parent;
        }

        public Argument(Expression expression, string name = null)
        {
            Expression = expression;
            Name = name;
        }

        public string Name { get; set; }

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

        ArgumentSyntax ISyntaxWrapper<ArgumentSyntax>.GetWrapped(ref bool? changed)
        {
            GetAndResetChanged(ref changed);

            bool? thisChanged = false;

            var newExpression = expression?.GetWrapped(ref thisChanged) ?? syntax.Expression;

            if (syntax == null || thisChanged == true || syntax.NameColon?.Name.Identifier.ValueText != Name || ShouldAnnotate(syntax, changed))
            {
                syntax = RoslynSyntaxFactory.Argument(
                    Name == null ? null : RoslynSyntaxFactory.NameColon(Name),
                    default, newExpression);

                syntax = Annotate(syntax);

                SetChanged(ref changed);
            }

            return syntax;
        }

        public static implicit operator Argument(Expression expression) => new Argument(expression);

        private protected override void SetSyntaxImpl(Roslyn::SyntaxNode newSyntax)
        {
            syntax = (ArgumentSyntax)newSyntax;
            expression = null;
        }

        private protected override SyntaxNode CloneImpl() => new Argument(Expression);

        public void ReplaceExpressions<T>(Func<T, bool> filter, Func<T, Expression> projection) where T : Expression =>
            Expression = Expression.ReplaceExpressions(Expression, filter, projection);
    }
}