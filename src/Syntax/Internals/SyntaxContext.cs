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

            // IdentifierExpression.AsTypeReference can give TypeReference that is not actually a type,
            // which means their symbol is not going to be an ITypeSymbol
            return (symbol ?? semanticModel.GetTypeInfo(typeSyntax).Type) as INamedTypeSymbol;
        }
    }
}