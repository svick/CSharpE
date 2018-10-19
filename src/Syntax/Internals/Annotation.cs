using System.Linq;
using Microsoft.CodeAnalysis;
using Roslyn = Microsoft.CodeAnalysis;

namespace CSharpE.Syntax.Internals
{
    public static class Annotation
    {
        private const string Kind = "CSharpE.Annotation";

        public static SyntaxAnnotation Create() => new SyntaxAnnotation(Kind);

        public static SyntaxAnnotation Get(Roslyn::SyntaxNode node) => node.GetAnnotations(Kind).FirstOrDefault();
    }
}
