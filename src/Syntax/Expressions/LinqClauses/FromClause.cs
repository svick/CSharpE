using System;
using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RoslynSyntaxFactory = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Roslyn = Microsoft.CodeAnalysis;

namespace CSharpE.Syntax
{
    public sealed class FromClause : LinqClause
    {
        private FromClauseSyntax syntax;

        internal FromClause(FromClauseSyntax syntax, SyntaxNode parent)
            : base(syntax)
        {
            Init(syntax);
            Parent = parent;
        }

        private void Init(FromClauseSyntax syntax)
        {
            this.syntax = syntax;
            name = new Identifier(syntax.Identifier);
        }

        public FromClause(string name, Expression expression)
            : this(null, name, expression) { }

        public FromClause(TypeReference type, string name, Expression expression)
        {
            Type = type;
            Name = name;
            Expression = expression;
        }

        private bool typeSet;
        private TypeReference type;
        public TypeReference Type
        {
            get
            {
                if (!typeSet)
                {
                    type = FromRoslyn.TypeReference(syntax.Type, this);
                    typeSet = true;
                }

                return type;
            }
            set
            {
                Set(ref type, value);
                typeSet = true;
            }
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

            var newType = typeSet ? type?.GetWrapped(ref thisChanged) : syntax.Type;
            var newName = name.GetWrapped(ref thisChanged);
            var newExpression = expression?.GetWrapped(ref thisChanged) ?? syntax.Expression;

            if (syntax == null || thisChanged == true)
            {
                syntax = RoslynSyntaxFactory.FromClause(newType, newName, newExpression);

                SetChanged(ref changed);
            }

            return syntax;
        }

        private protected override void SetSyntaxImpl(Roslyn::SyntaxNode newSyntax)
        {
            Init((FromClauseSyntax)newSyntax);
            Set(ref type, null);
            typeSet = false;
            Set(ref expression, null);
        }

        private protected override SyntaxNode CloneImpl() => new FromClause(Type, Name, Expression);

        public override void ReplaceExpressions<T>(Func<T, bool> filter, Func<T, Expression> projection) =>
            Expression = Expression.ReplaceExpressions(Expression, filter, projection);
    }
}
