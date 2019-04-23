using System.Collections.Generic;
using System.Linq;

namespace CSharpE.Syntax
{
    public static class SyntaxNodeExtensions
    {
        public static T Clone<T>(this T node) where T : SyntaxNode => (T)node.Clone();

        public static IList<T> Clone<T>(this IEnumerable<T> nodes) where T : SyntaxNode => nodes.Select(Clone).ToList();
    }
}