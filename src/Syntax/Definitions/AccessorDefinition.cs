using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis.CSharp.Syntax;

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
        
        // TODO: attributes, bodies (incl. expression bodies), modifiers

        AccessorDeclarationSyntax ISyntaxWrapper<AccessorDeclarationSyntax>.GetWrapped(ref bool? changed)
        {
            throw new System.NotImplementedException();
        }
        
        private protected override void SetSyntaxImpl(Microsoft.CodeAnalysis.SyntaxNode newSyntax)
        {
            throw new System.NotImplementedException();
        }

        internal override SyntaxNode Clone()
        {
            throw new System.NotImplementedException();
        }
    }
}