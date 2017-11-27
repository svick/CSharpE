using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSharpE.Syntax.Internals
{
    public struct SyntaxContext
    {
        private readonly SemanticModel semanticModel;

        internal SyntaxContext(SemanticModel semanticModel) => this.semanticModel =
            semanticModel ?? throw new ArgumentNullException(nameof(semanticModel));

        // TODO: is this actually correct?
        internal string GetFullName(TypeSyntax typeSyntax) =>
            semanticModel.GetTypeInfo(typeSyntax).Type.ToDisplayString();
    }
}