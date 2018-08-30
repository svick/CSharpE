using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSharpE.Syntax
{
    public struct NamespaceOrTypeDefinition : ISyntaxWrapper<MemberDeclarationSyntax>
    {
        private readonly ISyntaxWrapper<MemberDeclarationSyntax> namespaceOrType;

        public NamespaceOrTypeDefinition(NamespaceDefinition ns) => namespaceOrType = ns;
        public NamespaceOrTypeDefinition(BaseTypeDefinition type) => namespaceOrType = type;

        public bool IsNamespace => namespaceOrType is NamespaceDefinition;
        public bool IsType => namespaceOrType is BaseTypeDefinition;

        public NamespaceDefinition GetNamespaceDefinition() => (NamespaceDefinition)namespaceOrType;
        public BaseTypeDefinition GetTypeDefinition() => (BaseTypeDefinition)namespaceOrType;

        public SyntaxNode Value => (SyntaxNode)namespaceOrType;

        public static implicit operator NamespaceOrTypeDefinition(NamespaceDefinition ns) =>
            new NamespaceOrTypeDefinition(ns);
        public static implicit operator NamespaceOrTypeDefinition(BaseTypeDefinition type) =>
            new NamespaceOrTypeDefinition(type);

        MemberDeclarationSyntax ISyntaxWrapper<MemberDeclarationSyntax>.GetWrapped(ref bool? changed) =>
            namespaceOrType.GetWrapped(ref changed);
    }
}