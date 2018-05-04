using System;
using System.Collections.Generic;
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

        internal MemberAccessExpression(FieldDefinition fieldDefinition)
        {
            if (fieldDefinition.Modifiers.Contains(MemberModifiers.Static))
                Expression = fieldDefinition.ParentType;
            else
                Expression = This();

            MemberName = fieldDefinition.Name;
        }

        public MemberAccessExpression(FieldReference fieldReference)
        {
            if (fieldReference.IsStatic)
                Expression = fieldReference.ContainingType;
            else
                Expression = This();

            MemberName = fieldReference.Name;
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

        internal override ExpressionSyntax GetWrapped(WrapperContext context)
        {
            var newExpression = expression?.GetWrapped(context) ?? syntax.Expression;
            var newMemberName = memberName.GetWrapped(context);

            if (syntax == null || newExpression != syntax.Expression || newMemberName != syntax.Name.Identifier)
            {
                syntax = CSharpSyntaxFactory.MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression, newExpression,
                    CSharpSyntaxFactory.IdentifierName(newMemberName));
            }

            return syntax;
        }

        protected override IEnumerable<IEnumerable<SyntaxNode>> GetChildren()
        {
            yield return Node(Expression);
        }

        public override SyntaxNode Parent { get; internal set; }
    }
}