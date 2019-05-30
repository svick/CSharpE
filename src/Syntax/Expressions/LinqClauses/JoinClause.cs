using System;
using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RoslynSyntaxFactory = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Roslyn = Microsoft.CodeAnalysis;

namespace CSharpE.Syntax
{
    public sealed class JoinClause : LinqClause
    {
        private JoinClauseSyntax syntax;

        internal JoinClause(JoinClauseSyntax syntax, SyntaxNode parent)
            : base(syntax)
        {
            Init(syntax);
            Parent = parent;
        }

        private void Init(JoinClauseSyntax syntax)
        {
            this.syntax = syntax;
            name = new Identifier(syntax.Identifier);
            into = new Identifier(syntax.Into?.Identifier);
        }

        public JoinClause(
            string name, Expression inExpression, Expression leftExpression, Expression rightExpression,
            string into = null)
            : this(null, name, inExpression, leftExpression, rightExpression, into) { }

        public JoinClause(
            TypeReference type, string name, Expression inExpression,
            Expression leftExpression, Expression rightExpression, string into = null)
        {
            Type = type;
            Name = name;
            InExpression = inExpression;
            LeftExpression = leftExpression;
            RightExpression = rightExpression;
            this.into = new Identifier(into, true);
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

        private Expression inExpression;
        public Expression InExpression
        {
            get
            {
                if (inExpression == null)
                    inExpression = FromRoslyn.Expression(syntax.InExpression, this);

                return inExpression;
            }
            set => SetNotNull(ref inExpression, value);
        }

        private Expression leftExpression;
        public Expression LeftExpression
        {
            get
            {
                if (leftExpression == null)
                    leftExpression = FromRoslyn.Expression(syntax.LeftExpression, this);

                return leftExpression;
            }
            set => SetNotNull(ref leftExpression, value);
        }

        private Expression rightExpression;
        public Expression RightExpression
        {
            get
            {
                if (rightExpression == null)
                    rightExpression = FromRoslyn.Expression(syntax.RightExpression, this);

                return rightExpression;
            }
            set => SetNotNull(ref rightExpression, value);
        }

        private Identifier into;
        public string Into
        {
            get => into.Text;
            set => into.Text = value;
        }

        internal override Roslyn::SyntaxNode GetWrapped(ref bool? changed)
        {
            GetAndResetChanged(ref changed, out var thisChanged);

            var newType = typeSet ? type?.GetWrapped(ref thisChanged) : syntax.Type;
            var newName = name.GetWrapped(ref thisChanged);
            var newInExpression = inExpression?.GetWrapped(ref thisChanged) ?? syntax.InExpression;
            var newLeftExpression = leftExpression?.GetWrapped(ref thisChanged) ?? syntax.LeftExpression;
            var newRightExpression = rightExpression?.GetWrapped(ref thisChanged) ?? syntax.RightExpression;
            var newInto = into.GetWrapped(ref thisChanged);

            if (syntax == null || thisChanged == true)
            {
                var intoClause = newInto == default ? null : RoslynSyntaxFactory.JoinIntoClause(newInto);

                syntax = RoslynSyntaxFactory.JoinClause(
                    newType, newName, newInExpression, newLeftExpression, newRightExpression, intoClause);

                SetChanged(ref changed);
            }

            return syntax;
        }

        private protected override void SetSyntaxImpl(Roslyn::SyntaxNode newSyntax)
        {
            Init((JoinClauseSyntax)newSyntax);
            Set(ref type, null);
            typeSet = false;
            Set(ref inExpression, null);
            Set(ref leftExpression, null);
            Set(ref rightExpression, null);
        }

        private protected override SyntaxNode CloneImpl() =>
            new JoinClause(Type, Name, InExpression, LeftExpression, RightExpression, Into);

        public override void ReplaceExpressions<T>(Func<T, bool> filter, Func<T, Expression> projection)
        {
            InExpression = Expression.ReplaceExpressions(InExpression, filter, projection);
            LeftExpression = Expression.ReplaceExpressions(LeftExpression, filter, projection);
            RightExpression = Expression.ReplaceExpressions(RightExpression, filter, projection);
        }
    }
}
