using System;
using System.Collections.Generic;
using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Roslyn = Microsoft.CodeAnalysis;

namespace CSharpE.Syntax
{
    public class NamespaceDefinition : SyntaxNode, ISyntaxWrapper<NamespaceDeclarationSyntax>
    {
        public IList<NamespaceOrTypeDefinition> Members => throw new NotImplementedException();

        public NamespaceDefinition(NamespaceDeclarationSyntax ns, SyntaxNode parent)
        {
            throw new NotImplementedException();
        }

        NamespaceDeclarationSyntax ISyntaxWrapper<NamespaceDeclarationSyntax>.GetWrapped(ref bool? changed)
        {
            throw new NotImplementedException();
        }

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
