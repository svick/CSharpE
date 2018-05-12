using System.Collections.Generic;
using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSharpE.Syntax
{
    public class NamespaceDefinition : ISyntaxWrapper2<NamespaceDeclarationSyntax>
    {
        private SyntaxList<NamespaceOrTypeDefinition, MemberDeclarationSyntax> members;
        public IList<NamespaceOrTypeDefinition> Members => members;

        public NamespaceDefinition(NamespaceDeclarationSyntax ns)
        {
            throw new System.NotImplementedException();
        }

        public NamespaceDeclarationSyntax GetWrapped(ref bool changed)
        {
            throw new System.NotImplementedException();
        }
    }
}