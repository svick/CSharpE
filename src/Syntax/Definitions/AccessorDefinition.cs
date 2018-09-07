using System;
using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using CSharpSyntaxFactory = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace CSharpE.Syntax
{
    public sealed class AccessorDefinition : SyntaxNode, ISyntaxWrapper<AccessorDeclarationSyntax>
    {
        private AccessorDeclarationSyntax syntax;

        internal override SyntaxNode Parent { get; set; }
        
        internal AccessorDefinition(AccessorDeclarationSyntax syntax, MemberDefinition parent)
        {
            Init(syntax);
            Parent = parent;
        }

        private void Init(AccessorDeclarationSyntax syntax)
        {
            this.syntax = syntax;
        }

        public AccessorDefinition() { }

        private SyntaxKind? kind;
        internal SyntaxKind Kind
        {
            get => kind ?? syntax?.Kind() ?? throw new InvalidOperationException("Kind has not been set.");
            set
            {
                switch (value)
                {
                    case SyntaxKind.GetAccessorDeclaration:
                    case SyntaxKind.SetAccessorDeclaration:
                        break;
                    default:
                        throw new ArgumentException($"{value} is not allowed as accessor kind.");
                }

                kind = value;
            }
        }

        // TODO: attributes, bodies (incl. expression bodies), modifiers

        AccessorDeclarationSyntax ISyntaxWrapper<AccessorDeclarationSyntax>.GetWrapped(ref bool? changed)
        {
            GetAndResetChanged(ref changed);

            var newKind = Kind;

            if (syntax == null || syntax.Kind() != Kind || !IsAnnotated(syntax))
            {
                var newSyntax = CSharpSyntaxFactory.AccessorDeclaration(newKind)
                    .WithSemicolonToken(CSharpSyntaxFactory.Token(SyntaxKind.SemicolonToken));

                syntax = Annotate(newSyntax);
                
                SetChanged(ref changed);
            }

            return syntax;
        }
        
        private protected override void SetSyntaxImpl(Microsoft.CodeAnalysis.SyntaxNode newSyntax)
        {
            throw new System.NotImplementedException();
        }

        internal override SyntaxNode Clone() => new AccessorDefinition();
    }
}