using System;
using System.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSharpE.Syntax.Internals
{
    public struct SyntaxContext
    {
        private readonly SemanticModel semanticModel;

        internal SyntaxContext(SemanticModel semanticModel) =>
            this.semanticModel = semanticModel ?? throw new ArgumentNullException(nameof(semanticModel));

        internal INamedTypeSymbol Resolve(TypeSyntax typeSyntax)
        {
            var symbol = semanticModel.GetSymbolInfo(typeSyntax).Symbol;

            // this happens for attributes
            if (symbol is IMethodSymbol methodSymbol)
            {
                Debug.Assert(methodSymbol.MethodKind == MethodKind.Constructor);
                symbol = methodSymbol.ContainingType;
            }

            return (INamedTypeSymbol)(symbol ?? semanticModel.GetTypeInfo(typeSyntax).Type);
        }
    }
}