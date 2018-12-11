using System;
using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using CSharpSyntaxFactory = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Roslyn = Microsoft.CodeAnalysis;

namespace CSharpE.Syntax
{
    // TODO: ref, out and in
    public sealed class Argument : SyntaxNode, ISyntaxWrapper<ArgumentSyntax>
    {
        private ArgumentSyntax syntax;

        internal Argument(ArgumentSyntax syntax, SyntaxNode parent)
        {
            this.syntax = syntax ?? throw new ArgumentNullException(nameof(syntax));
            name = new Identifier(syntax.NameColon?.Name.Identifier ?? default, canBeNull: true);
            Parent = parent;
        }

        public Argument(Expression expression, string name = null)
        {
            Expression = expression;
            this.name = new Identifier(name, canBeNull: true);
        }

        private Identifier name;
        public string Name
        {
            get => name.Text;
            set => name.Text = value;
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

        ArgumentSyntax ISyntaxWrapper<ArgumentSyntax>.GetWrapped(ref bool? changed)
        {
            GetAndResetChanged(ref changed);

            bool? thisChanged = false;

            var newName = name.GetWrapped(ref thisChanged);
            var newExpression = expression?.GetWrapped(ref thisChanged) ?? syntax.Expression;

            if (syntax == null || thisChanged == true)
            {
                syntax = CSharpSyntaxFactory.Argument(
                    newName == default ? null : CSharpSyntaxFactory.NameColon(CSharpSyntaxFactory.IdentifierName(newName)),
                    default, newExpression);

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

        internal override SyntaxNode Clone() => new Argument(Expression);

        internal override SyntaxNode Parent { get; set; }
    }
}