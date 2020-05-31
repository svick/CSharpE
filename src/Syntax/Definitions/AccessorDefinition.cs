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

        public AccessorDefinition() { }

        public AccessorDefinition(MemberModifiers accessibility)
            : this(accessibility, null) { }

        public AccessorDefinition(BlockStatement body)
            : this(default, body) { }

        public AccessorDefinition(MemberModifiers accessibility, BlockStatement body)
        {
            Accessibility = accessibility;
            Body = body;
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
                    attributes = new SyntaxList<Attribute, AttributeListSyntax>(syntax.AttributeLists, this);

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

        // TODO: expression body

        private bool bodySet;
        private BlockStatement body;
        public BlockStatement Body
        {
            get
            {
                if (!bodySet)
                {
                    body = syntax.Body == null ? null : new BlockStatement(syntax.Body, this);
                    bodySet = true;
                }

                return body;
            }
            set
            {
                Set(ref body, value);
                bodySet = true;
            }
        }

        AccessorDeclarationSyntax ISyntaxWrapper<AccessorDeclarationSyntax>.GetWrapped(ref bool? changed)
        {
            GetAndResetChanged(ref changed, out var thisChanged);

            var newKind = Kind;
            var newAttributes = attributes?.GetWrapped(ref thisChanged) ?? syntax?.AttributeLists ?? default;
            var newModifiers = modifiers;
            var newBody = bodySet ? body?.GetWrapped(ref thisChanged) : syntax.Body;

            if (syntax == null || thisChanged == true || newModifiers != FromRoslyn.MemberModifiers(syntax.Modifiers) || syntax.Kind() != Kind
                || ShouldAnnotate(syntax, changed))
            {
                var newSyntax = RoslynSyntaxFactory.AccessorDeclaration(newKind, newAttributes, newModifiers.GetWrapped(), newBody);

                if (newBody == null)
                    newSyntax = newSyntax.WithSemicolonToken(RoslynSyntaxFactory.Token(SyntaxKind.SemicolonToken));

                syntax = Annotate(newSyntax);

                SetChanged(ref changed);
            }

            return syntax;
        }

        private protected override void SetSyntaxImpl(Roslyn::SyntaxNode newSyntax)
        {
            Init((AccessorDeclarationSyntax)newSyntax);

            SetList(ref attributes, null);
            Set(ref body, null);
        }

        private protected override SyntaxNode CloneImpl() => new AccessorDefinition(Accessibility, Body) { Attributes = Attributes };

        public void ReplaceExpressions<T>(Func<T, bool> filter, Func<T, Expression> projection) where T : Expression =>
            Body.ReplaceExpressions(filter, projection);
    }
}