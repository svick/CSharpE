﻿using System;
using System.Collections.Generic;
using System.Linq;
using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
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

        private SyntaxNode parent;

        internal BaseTypeDefinition() { }

        internal BaseTypeDefinition(MemberDeclarationSyntax memberDeclarationSyntax)
            : base(memberDeclarationSyntax) { }

        internal override SyntaxNode Parent
        {
            get => parent;
            set
            {
                switch (value)
                {
                    case TypeDefinition _:
                    case NamespaceDefinition _:
                    case SourceFile _:
                        parent = value;
                        return;
                }

                throw new ArgumentException(nameof(value));
            }
        }

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
                    case BaseTypeDefinition typeDefinition:
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