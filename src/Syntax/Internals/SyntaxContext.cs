using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSharpE.Syntax.Internals
{
    public struct SyntaxContext
    {
        private readonly SemanticModel semanticModel;

        internal SyntaxContext(SemanticModel semanticModel) =>
            this.semanticModel = semanticModel ?? throw new ArgumentNullException(nameof(semanticModel));

        internal INamedTypeSymbol Resolve(TypeSyntax typeSyntax) =>
            (INamedTypeSymbol)semanticModel.GetTypeInfo(typeSyntax).Type;
    }
}