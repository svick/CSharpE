using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSharpE.Syntax.Internals
{
    internal struct SyntaxContext
    {
        private readonly SemanticModel semanticModel;

        public SyntaxContext(SemanticModel semanticModel)
        {
            this.semanticModel = semanticModel;
        }

        // TODO: is this actually correct?
        public string GetFullName(TypeSyntax typeSyntax) =>
            semanticModel.GetTypeInfo(typeSyntax).Type.ToDisplayString();
    }
}