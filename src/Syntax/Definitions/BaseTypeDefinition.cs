using System;
using System.Collections.Generic;
using System.Linq;
using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSharpE.Syntax
{
    public abstract class BaseTypeDefinition : MemberDefinition
    {
        private protected Identifier name;
        public string Name
        {
            get => name.Text;
            set => name.Text = value;
        }

        internal BaseTypeDefinition() { }

        internal BaseTypeDefinition(MemberDeclarationSyntax memberDeclarationSyntax)
            : base(memberDeclarationSyntax) { }

        public INamedTypeSymbol GetSymbol() =>
            (INamedTypeSymbol)SourceFile.SemanticModel.GetDeclaredSymbol(GetSourceFileNode());

        public NamedTypeReference GetReference() => new NamedTypeReference(GetSymbol());

        internal string GetNamespace()
        {
            var namespaceParts = new List<string>();

            SyntaxNode node = this;

            while (node != null)
            {
                switch (node)
                {
                    case BaseTypeDefinition _:
                        node = node.Parent;
                        break;
                    case NamespaceDefinition namespaceDefinition:
                        namespaceParts.Add(namespaceDefinition.Name);
                        node = node.Parent;
                        break;
                    case SourceFile _:
                        return string.Join(".", namespaceParts.Reverse<string>());
                    default:
                        throw new InvalidOperationException();
                }
            }

            return null;
        }
    }
}