using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSharpE.Syntax
{
    public struct NamespaceOrTypeDefinition : ISyntaxWrapper<MemberDeclarationSyntax>
    {
        private readonly ISyntaxWrapper<MemberDeclarationSyntax> namespaceOrType;

        public NamespaceOrTypeDefinition(NamespaceDefinition ns) => namespaceOrType = ns;
        public NamespaceOrTypeDefinition(TypeDefinition type) => namespaceOrType = type;

        public bool IsNamespace => namespaceOrType is NamespaceDefinition;
        public bool IsType => namespaceOrType is TypeDefinition;

        public NamespaceDefinition GetNamespaceDefinition() => (NamespaceDefinition)namespaceOrType;
        public TypeDefinition GetTypeDefinition() => (TypeDefinition)namespaceOrType;

        public SyntaxNode Value => (SyntaxNode)namespaceOrType;

        public static implicit operator NamespaceOrTypeDefinition(NamespaceDefinition ns) =>
            new NamespaceOrTypeDefinition(ns);
        public static implicit operator NamespaceOrTypeDefinition(TypeDefinition type) =>
            new NamespaceOrTypeDefinition(type);

        MemberDeclarationSyntax ISyntaxWrapper<MemberDeclarationSyntax>.GetWrapped(ref bool? changed) =>
            namespaceOrType.GetWrapped(ref changed);
    }
}