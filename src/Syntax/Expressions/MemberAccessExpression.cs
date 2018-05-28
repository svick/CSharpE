using System;
using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static CSharpE.Syntax.SyntaxFactory;
using CSharpSyntaxFactory = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Roslyn = Microsoft.CodeAnalysis;

namespace CSharpE.Syntax
{
    // TODO: generics
    public sealed class MemberAccessExpression : Expression
    {
        private MemberAccessExpressionSyntax syntax;

        internal MemberAccessExpression(MemberAccessExpressionSyntax syntax, SyntaxNode parent)
        {
            Init(syntax);
            Parent = parent;
        }

        private void Init(MemberAccessExpressionSyntax memberAccessExpressionSyntax)
        {
            syntax = memberAccessExpressionSyntax;

            memberName = new Identifier(memberAccessExpressionSyntax.Name.Identifier);
        }

        public MemberAccessExpression(Expression expression, string memberName)
        {
            Expression = expression;
            MemberName = memberName;
        }

        public MemberAccessExpression(FieldDefinition fieldDefinition)
        {
            if (fieldDefinition.Modifiers.Contains(MemberModifiers.Static))
                Expression = fieldDefinition.ParentType;
            else
                Expression = This();

            MemberName = fieldDefinition.Name;
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

        private Identifier memberName;
        public string MemberName
        {
            get => memberName.Text;
            set => memberName.Text = value;
        }

        internal override ExpressionSyntax GetWrapped(ref bool? changed)
        {
            GetAndResetChanged(ref changed);

            bool? thisChanged = false;

            var newExpression = expression?.GetWrapped(ref thisChanged) ?? syntax.Expression;
            var newMemberName = memberName.GetWrapped(ref thisChanged);

            if (syntax == null || thisChanged == true)
            {
                syntax = CSharpSyntaxFactory.MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression, newExpression,
                    CSharpSyntaxFactory.IdentifierName(newMemberName));

                SetChanged(ref changed);
            }

            return syntax;
        }

        protected override void SetSyntaxImpl(Roslyn::SyntaxNode newSyntax)
        {
            Init((MemberAccessExpressionSyntax)newSyntax);

            Set(ref expression, null);
        }

        internal override SyntaxNode Clone() => new MemberAccessExpression(Expression, MemberName);

        internal override SyntaxNode Parent { get; set; }
    }
}