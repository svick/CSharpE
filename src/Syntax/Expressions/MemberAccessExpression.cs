using System;
using System.Collections.Generic;
using System.Diagnostics;
using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Roslyn = Microsoft.CodeAnalysis;
using RoslynSyntaxFactory = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace CSharpE.Syntax
{
    public abstract class BaseMemberAccessExpression : Expression
    {
        private ExpressionSyntax syntax;

        internal BaseMemberAccessExpression(ExpressionSyntax syntax, SyntaxNode parent)
            : base(syntax)
        {
            Init(syntax);
            Parent = parent;
        }

        private void Init(ExpressionSyntax syntax)
        {
            this.syntax = syntax;

            SimpleNameSyntax nameSyntax;

            switch (syntax)
            {
                case MemberAccessExpressionSyntax memberAccess:
                    nameSyntax = memberAccess.Name;
                    break;
                case ConditionalAccessExpressionSyntax conditionalAccess:
                    var memberBinding = (MemberBindingExpressionSyntax)conditionalAccess.WhenNotNull;
                    nameSyntax = memberBinding.Name;
                    break;
                default:
                    throw new InvalidOperationException();
            }

            memberName = new Identifier(nameSyntax.Identifier);

            var typeArgumentList = nameSyntax is GenericNameSyntax genericNameSyntax ? genericNameSyntax.TypeArgumentList.Arguments : default;
            typeArguments = new SeparatedSyntaxList<TypeReference, TypeSyntax>(typeArgumentList, this);
        }

        internal BaseMemberAccessExpression(Expression expression, string memberName)
            : this(expression, memberName, null) { }

        internal BaseMemberAccessExpression(Expression expression, string memberName, IEnumerable<TypeReference> typeArguments)
        {
            Expression = expression;
            MemberName = memberName;
            this.typeArguments = new SeparatedSyntaxList<TypeReference, TypeSyntax>(typeArguments, this);
        }

        private ExpressionSyntax GetExpressionSyntax()
        {
            switch (syntax)
            {
                case MemberAccessExpressionSyntax memberAccess:
                    return memberAccess.Expression;
                case ConditionalAccessExpressionSyntax conditionalAccess:
                    return conditionalAccess.Expression;
                default:
                    throw new InvalidOperationException();
            }
        }

        private Expression expression;
        public Expression Expression
        {
            get
            {
                if (expression == null)
                    expression = FromRoslyn.Expression(GetExpressionSyntax(), this);

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

        private SeparatedSyntaxList<TypeReference, TypeSyntax> typeArguments;
        public IList<TypeReference> TypeArguments
        {
            get => typeArguments;
            set => SetList(ref typeArguments, new SeparatedSyntaxList<TypeReference, TypeSyntax>(value, this));
        }

        private protected override ExpressionSyntax GetWrappedExpression(ref bool? changed)
        {
            GetAndResetChanged(ref changed);

            bool? thisChanged = false;

            var newExpression = expression?.GetWrapped(ref thisChanged) ?? GetExpressionSyntax();
            var newMemberName = memberName.GetWrapped(ref thisChanged);
            var newTypeArguments = typeArguments.GetWrapped(ref thisChanged);

            if (syntax == null || thisChanged == true || !IsAnnotated(syntax))
            {
                var nameSyntax = newTypeArguments.Any()
                    ? (SimpleNameSyntax)RoslynSyntaxFactory.GenericName(
                        newMemberName, RoslynSyntaxFactory.TypeArgumentList(newTypeArguments))
                    : RoslynSyntaxFactory.IdentifierName(newMemberName);

                if (this is ConditionalMemberAccessExpression)
                {
                    syntax = RoslynSyntaxFactory.ConditionalAccessExpression(
                        newExpression, RoslynSyntaxFactory.MemberBindingExpression(nameSyntax));
                }
                else
                {
                    syntax = RoslynSyntaxFactory.MemberAccessExpression(
                        this is PointerMemberAccessExpression
                            ? SyntaxKind.PointerMemberAccessExpression
                            : SyntaxKind.SimpleMemberAccessExpression,
                        newExpression, nameSyntax);
                }

                syntax = Annotate(syntax);

                SetChanged(ref changed);
            }

            return syntax;
        }

        private protected override void SetSyntaxImpl(Roslyn::SyntaxNode newSyntax)
        {
            Init((ExpressionSyntax)newSyntax);

            Set(ref expression, null);
        }
    }

    public class MemberAccessExpression : BaseMemberAccessExpression
    {
        internal MemberAccessExpression(MemberAccessExpressionSyntax syntax, SyntaxNode parent)
            : base(syntax, parent)
            => Debug.Assert(syntax.Kind() == SyntaxKind.SimpleMemberAccessExpression);

        public MemberAccessExpression(Expression expression, string memberName) : base(expression, memberName) { }

        public MemberAccessExpression(Expression expression, string memberName, params TypeReference[] typeArguments)
            : base(expression, memberName, typeArguments) { }

        public MemberAccessExpression(Expression expression, string memberName, IEnumerable<TypeReference> typeArguments)
            : base(expression, memberName, typeArguments) { }

        public MemberAccessExpression(Expression expression, FieldDefinition fieldDefinition)
            : this(expression, fieldDefinition.Name) { }

        private protected override SyntaxNode CloneImpl() => new MemberAccessExpression(Expression, MemberName, TypeArguments);
    }

    public class PointerMemberAccessExpression : BaseMemberAccessExpression
    {
        internal PointerMemberAccessExpression(MemberAccessExpressionSyntax syntax, SyntaxNode parent)
            : base(syntax, parent)
            => Debug.Assert(syntax.Kind() == SyntaxKind.PointerMemberAccessExpression);

        public PointerMemberAccessExpression(Expression expression, string memberName)
            : base(expression, memberName) { }

        public PointerMemberAccessExpression(Expression expression, string memberName, params TypeReference[] typeArguments)
            : base(expression, memberName, typeArguments) { }

        public PointerMemberAccessExpression(Expression expression, string memberName, IEnumerable<TypeReference> typeArguments)
            : base(expression, memberName, typeArguments) { }

        public PointerMemberAccessExpression(Expression expression, FieldDefinition fieldDefinition)
            : this(expression, fieldDefinition.Name) { }

        private protected override SyntaxNode CloneImpl() => new PointerMemberAccessExpression(Expression, MemberName, TypeArguments);
    }

    public class ConditionalMemberAccessExpression : BaseMemberAccessExpression
    {
        internal ConditionalMemberAccessExpression(ConditionalAccessExpressionSyntax syntax, SyntaxNode parent)
            : base(syntax, parent)
            => Debug.Assert(syntax.WhenNotNull is MemberBindingExpressionSyntax);

        public ConditionalMemberAccessExpression(Expression expression, string memberName)
            : base(expression, memberName) { }

        public ConditionalMemberAccessExpression(Expression expression, string memberName, params TypeReference[] typeArguments)
            : base(expression, memberName, typeArguments) { }

        public ConditionalMemberAccessExpression(Expression expression, string memberName, IEnumerable<TypeReference> typeArguments)
            : base(expression, memberName, typeArguments) { }

        public ConditionalMemberAccessExpression(Expression expression, FieldDefinition fieldDefinition)
            : this(expression, fieldDefinition.Name) { }

        private protected override SyntaxNode CloneImpl() => new ConditionalMemberAccessExpression(Expression, MemberName, TypeArguments);
    }
}