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

        internal NamespaceDeclarationSyntax GetWrapped(ref bool? changed)
        {
            throw new System.NotImplementedException();
        }

        NamespaceDeclarationSyntax ISyntaxWrapper<NamespaceDeclarationSyntax>.GetWrapped(ref bool? changed) =>
            GetWrapped(ref changed);
    }
}