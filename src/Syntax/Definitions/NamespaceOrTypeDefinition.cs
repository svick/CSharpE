using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSharpE.Syntax
{
    public struct NamespaceOrTypeDefinition : ISyntaxWrapper<MemberDeclarationSyntax>
    {
        internal readonly ISyntaxWrapper<MemberDeclarationSyntax> NamespaceOrType;

        internal NamespaceOrTypeDefinition(ISyntaxWrapper<MemberDeclarationSyntax> namespaceOrType) =>
            NamespaceOrType = namespaceOrType;

        public NamespaceOrTypeDefinition(NamespaceDefinition ns) => NamespaceOrType = ns;
        public NamespaceOrTypeDefinition(BaseTypeDefinition type) => NamespaceOrType = type;

        public bool IsNamespace => NamespaceOrType is NamespaceDefinition;
        public bool IsType => NamespaceOrType is BaseTypeDefinition;

        public NamespaceDefinition GetNamespaceDefinition() => (NamespaceDefinition)NamespaceOrType;
        public BaseTypeDefinition GetTypeDefinition() => (BaseTypeDefinition)NamespaceOrType;

        public SyntaxNode Value => (SyntaxNode)NamespaceOrType;

        public static implicit operator NamespaceOrTypeDefinition(NamespaceDefinition ns) =>
            new NamespaceOrTypeDefinition(ns);
        public static implicit operator NamespaceOrTypeDefinition(BaseTypeDefinition type) =>
            new NamespaceOrTypeDefinition(type);

        MemberDeclarationSyntax ISyntaxWrapper<MemberDeclarationSyntax>.GetWrapped(ref bool? changed) =>
            NamespaceOrType.GetWrapped(ref changed);
    }
}