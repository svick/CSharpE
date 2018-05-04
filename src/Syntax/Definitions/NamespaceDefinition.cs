using System.Collections.Generic;
using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSharpE.Syntax
{
    public class NamespaceDefinition : ISyntaxWrapper<NamespaceDeclarationSyntax>
    {
        private SyntaxList<NamespaceOrTypeDefinition, MemberDeclarationSyntax> members;
        public IList<NamespaceOrTypeDefinition> Members => members;

        public NamespaceDefinition(NamespaceDeclarationSyntax ns)
        {
            throw new System.NotImplementedException();
        }

        public NamespaceDeclarationSyntax GetWrapped()
        {
            throw new System.NotImplementedException();
        }
    }
}