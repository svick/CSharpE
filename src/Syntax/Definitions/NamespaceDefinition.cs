using System;
using System.Collections.Generic;
using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Roslyn = Microsoft.CodeAnalysis;

namespace CSharpE.Syntax
{
    public class NamespaceDefinition : SyntaxNode, ISyntaxWrapper<NamespaceDeclarationSyntax>
    {
        private SyntaxList<NamespaceOrTypeDefinition, MemberDeclarationSyntax> members;
        public IList<NamespaceOrTypeDefinition> Members => members;

        public NamespaceDefinition(NamespaceDeclarationSyntax ns)
        {
            throw new System.NotImplementedException();
        }

        internal NamespaceDeclarationSyntax GetWrapped(ref bool? changed)
        {
            throw new System.NotImplementedException();
        }

        NamespaceDeclarationSyntax ISyntaxWrapper<NamespaceDeclarationSyntax>.GetWrapped(ref bool? changed) =>
            GetWrapped(ref changed);

        private NamespaceDefinition parent;
        internal override SyntaxNode Parent
        {
            get => parent;
            set
            {
                if (value is NamespaceDefinition parentNamespace)
                    parent = parentNamespace;
                else
                    throw new ArgumentException(nameof(value));
            }
        }

        private protected override void SetSyntaxImpl(Roslyn::SyntaxNode newSyntax)
        {
            throw new NotImplementedException();
        }

        internal override SyntaxNode Clone()
        {
            throw new NotImplementedException();
        }
    }
}