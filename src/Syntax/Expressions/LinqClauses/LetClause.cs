using System;
using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RoslynSyntaxFactory = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Roslyn = Microsoft.CodeAnalysis;

namespace CSharpE.Syntax
{
    public sealed class LetClause : LinqClause
    {
        private LetClauseSyntax syntax;

        internal LetClause(LetClauseSyntax syntax, SyntaxNode parent)
            : base(syntax)
        {
            Init(syntax);
            Parent = parent;
        }

        private void Init(LetClauseSyntax syntax)
        {
            this.syntax = syntax;
            name = new Identifier(syntax.Identifier);
        }

        public LetClause(string name, Expression expression)
        {
            Name = name;
            Expression = expression;
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

        internal override Roslyn::SyntaxNode GetWrapped(ref bool? changed)
        {
            GetAndResetChanged(ref changed, out var thisChanged);

            var newName = name.GetWrapped(ref thisChanged);
            var newExpression = expression?.GetWrapped(ref thisChanged) ?? syntax.Expression;

            if (syntax == null || thisChanged == true)
            {
                syntax = RoslynSyntaxFactory.LetClause(newName, newExpression);

                SetChanged(ref changed);
            }

            return syntax;
        }

        private protected override void SetSyntaxImpl(Roslyn::SyntaxNode newSyntax)
        {
            Init((LetClauseSyntax)newSyntax);
            Set(ref expression, null);
        }

        private protected override SyntaxNode CloneImpl() => new LetClause(Name, Expression);

        public override void ReplaceExpressions<T>(Func<T, bool> filter, Func<T, Expression> projection) =>
            Expression = Expression.ReplaceExpressions(Expression, filter, projection);
    }
}
