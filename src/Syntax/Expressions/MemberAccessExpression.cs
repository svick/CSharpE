using System;
using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static CSharpE.Syntax.SyntaxFactory;
using CSharpSyntaxFactory = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace CSharpE.Syntax
{
    // TODO: generics
    public sealed class MemberAccessExpression : Expression
    {
        private MemberAccessExpressionSyntax syntax;

        internal MemberAccessExpression(MemberAccessExpressionSyntax syntax, SyntaxNode parent)
        {
            this.syntax = syntax;
            Parent = parent;

            memberName = new Identifier(syntax.Name.Identifier);
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
                    expression = FromRoslyn.Expression(syntax.Expression);

                return expression;
            }
            set => expression = value ?? throw new ArgumentNullException(nameof(value));
        }

        private Identifier memberName;
        public string MemberName
        {
            get => memberName.Text;
            set => memberName.Text = value;
        }

        internal override ExpressionSyntax GetWrapped()
        {
            var newExpression = expression?.GetWrapped() ?? syntax.Expression;
            var newMemberName = memberName.GetWrapped();

            if (syntax == null || newExpression != syntax.Expression || newMemberName != syntax.Name.Identifier)
            {
                syntax = CSharpSyntaxFactory.MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression, newExpression,
                    CSharpSyntaxFactory.IdentifierName(newMemberName));
            }

            return syntax;
        }

        internal override SyntaxNode Parent { get; set; }
    }
}