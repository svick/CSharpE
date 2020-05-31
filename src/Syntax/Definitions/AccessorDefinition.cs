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
        {
            Accessibility = accessibility;
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

        // TODO: bodies (incl. expression bodies)

        AccessorDeclarationSyntax ISyntaxWrapper<AccessorDeclarationSyntax>.GetWrapped(ref bool? changed)
        {
            GetAndResetChanged(ref changed);

            var newKind = Kind;

            if (syntax == null || syntax.Kind() != Kind || ShouldAnnotate(syntax, changed))
            {
                var newSyntax = RoslynSyntaxFactory.AccessorDeclaration(newKind)
                    .WithSemicolonToken(RoslynSyntaxFactory.Token(SyntaxKind.SemicolonToken));

                syntax = Annotate(newSyntax);
                
                SetChanged(ref changed);
            }

            return syntax;
        }

        private MemberModifiers modifiers;
        public MemberModifiers Accessibility
        {
            get => modifiers;
            set => modifiers = modifiers.WithAccessibilityModifier(value);
        }

        private protected override void SetSyntaxImpl(Roslyn::SyntaxNode newSyntax)
        {
            Init((AccessorDeclarationSyntax)newSyntax);

            SetList(ref attributes, null);
        }

        private protected override SyntaxNode CloneImpl() => new AccessorDefinition(Accessibility) { Attributes = Attributes };

        public void ReplaceExpressions<T>(Func<T, bool> filter, Func<T, Expression> projection) where T : Expression { }
    }
}