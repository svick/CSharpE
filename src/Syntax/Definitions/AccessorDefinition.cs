using System;
using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RoslynSyntaxFactory = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Roslyn = Microsoft.CodeAnalysis;

namespace CSharpE.Syntax
{
    public sealed class AccessorDefinition : SyntaxNode, ISyntaxWrapper<AccessorDeclarationSyntax>
    {
        private AccessorDeclarationSyntax syntax;

        internal override SyntaxNode Parent { get; set; }
        
        internal AccessorDefinition(AccessorDeclarationSyntax syntax, MemberDefinition parent)
        {
            this.syntax = syntax;
            Parent = parent;
        }

        public AccessorDefinition() { }

        private SyntaxKind? kind;
        internal SyntaxKind Kind
        {
            get => kind ?? syntax.Kind();
            set
            {
                if (value != SyntaxKind.GetAccessorDeclaration && value != SyntaxKind.SetAccessorDeclaration)
                    throw new ArgumentException($"{value} is not allowed as accessor kind.");

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
                var newSyntax = RoslynSyntaxFactory.AccessorDeclaration(newKind)
                    .WithSemicolonToken(RoslynSyntaxFactory.Token(SyntaxKind.SemicolonToken));

                syntax = Annotate(newSyntax);
                
                SetChanged(ref changed);
            }

            return syntax;
        }

        private protected override void SetSyntaxImpl(Roslyn::SyntaxNode newSyntax) =>
            syntax = (AccessorDeclarationSyntax)newSyntax;

        private protected override SyntaxNode CloneImpl() => new AccessorDefinition();

        public void ReplaceExpressions<T>(Func<T, bool> filter, Func<T, Expression> projection) where T : Expression { }
    }
}