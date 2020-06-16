using System;
using System.Collections.Generic;
using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RoslynSyntaxFactory = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Roslyn = Microsoft.CodeAnalysis;

namespace CSharpE.Syntax
{
    public sealed class AccessorDefinition : SyntaxNode, ISyntaxWrapper<AccessorDeclarationSyntax>, IHasAttributes
    {
        private AccessorDeclarationSyntax syntax;

        internal AccessorDefinition(AccessorDeclarationSyntax syntax, MemberDefinition parent)
            : base(syntax)
        {
            Init(syntax);
            Parent = parent;
        }

        private void Init(AccessorDeclarationSyntax syntax)
        {
            this.syntax = syntax;

            Accessibility = FromRoslyn.MemberModifiers(syntax.Modifiers);
        }

        public AccessorDefinition(BlockStatement statementBody = null)
            : this(default, statementBody) { }

        public AccessorDefinition(MemberModifiers accessibility, BlockStatement statementBody = null)
            : this(accessibility, statementBody, null) { }

        public AccessorDefinition(Expression expressionBody)
            : this(default, expressionBody) { }

        public AccessorDefinition(MemberModifiers accessibility, Expression expressionBody)
            : this(accessibility, null, expressionBody) { }

        private AccessorDefinition(MemberModifiers accessibility, BlockStatement statementBody, Expression expressionBody)
        {
            Accessibility = accessibility;
            StatementBody = statementBody;
            ExpressionBody = expressionBody;
        }

        private SyntaxKind? kind;
        internal SyntaxKind Kind
        {
            get => kind ?? syntax.Kind();
            set
            {
                switch (value)
                {
                    case SyntaxKind.GetAccessorDeclaration:
                    case SyntaxKind.SetAccessorDeclaration:
                    case SyntaxKind.AddAccessorDeclaration:
                    case SyntaxKind.RemoveAccessorDeclaration:
                        kind = value;
                        break;
                    default:
                        throw new ArgumentException($"{value} is not allowed as accessor kind.");
                }
            }
        }

        private SyntaxList<Attribute, AttributeListSyntax> attributes;
        public IList<Attribute> Attributes
        {
            get
            {
                if (attributes == null)
                    attributes = new SyntaxList<Attribute, AttributeListSyntax>(syntax?.AttributeLists ?? default, this);

                return attributes;
            }
            set => SetList(ref attributes, new SyntaxList<Attribute, AttributeListSyntax>(value, this));
        }

        private MemberModifiers modifiers;
        public MemberModifiers Accessibility
        {
            get => modifiers;
            set => modifiers = modifiers.WithAccessibilityModifier(value);
        }

        private bool statementBodySet;
        private BlockStatement statementBody;
        public BlockStatement StatementBody
        {
            get
            {
                if (!statementBodySet)
                {
                    statementBody = syntax.Body == null ? null : new BlockStatement(syntax.Body, this);
                    statementBodySet = true;
                }

                return statementBody;
            }
            set
            {
                Set(ref statementBody, value);
                statementBodySet = true;

                if (value != null)
                    ExpressionBody = null;
            }
        }

        private bool expressionBodySet;
        private Expression expressionBody;
        public Expression ExpressionBody
        {
            get
            {
                if (!expressionBodySet)
                {
                    expressionBody = syntax.ExpressionBody == null
                        ? null
                        : FromRoslyn.Expression(syntax.ExpressionBody.Expression, this);
                    expressionBodySet = true;
                }

                return expressionBody;
            }
            set
            {
                Set(ref expressionBody, value);
                expressionBodySet = true;

                if (value != null)
                    StatementBody = null;
            }
        }

        AccessorDeclarationSyntax ISyntaxWrapper<AccessorDeclarationSyntax>.GetWrapped(ref bool? changed)
        {
            GetAndResetChanged(ref changed, out var thisChanged);

            var newKind = Kind;
            var newAttributes = attributes?.GetWrapped(ref thisChanged) ?? syntax?.AttributeLists ?? default;
            var newModifiers = modifiers;
            var newStatementBody = statementBodySet ? statementBody?.GetWrapped(ref thisChanged) : syntax.Body;
            var newExpressionBody = expressionBodySet ? expressionBody?.GetWrapped(ref thisChanged) : syntax.ExpressionBody?.Expression;

            if (syntax == null || thisChanged == true || newModifiers != FromRoslyn.MemberModifiers(syntax.Modifiers) || syntax.Kind() != Kind
                || ShouldAnnotate(syntax, changed))
            {
                var arrowClause = newExpressionBody == null ? null : RoslynSyntaxFactory.ArrowExpressionClause(newExpressionBody);
                var semicolonToken = newStatementBody == null ? RoslynSyntaxFactory.Token(SyntaxKind.SemicolonToken) : default;

                var newSyntax = RoslynSyntaxFactory.AccessorDeclaration(
                    newKind, newAttributes, newModifiers.GetWrapped(), newStatementBody, arrowClause)
                    .WithSemicolonToken(semicolonToken);

                syntax = Annotate(newSyntax);

                SetChanged(ref changed);
            }

            return syntax;
        }

        private protected override void SetSyntaxImpl(Roslyn::SyntaxNode newSyntax)
        {
            Init((AccessorDeclarationSyntax)newSyntax);

            SetList(ref attributes, null);
            Set(ref statementBody, null);
            statementBodySet = false;
            Set(ref expressionBody, null);
            expressionBodySet = false;
        }

        private protected override SyntaxNode CloneImpl() => 
            new AccessorDefinition(Accessibility, StatementBody, ExpressionBody) { Attributes = Attributes };

        public void ReplaceExpressions<T>(Func<T, bool> filter, Func<T, Expression> projection) where T : Expression
        {
            StatementBody?.ReplaceExpressions(filter, projection);
            ExpressionBody = Expression.ReplaceExpressions(ExpressionBody, filter, projection);
        }
    }
}