using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace CSharpE.Syntax.Internals
{
    internal static class SyntaxTreeExtensions
    {
        public static IReadOnlyList<TextChangeRange> GetChangeRanges(this SyntaxTree newTree, SyntaxTree oldTree)
        {
            // PERF: some form of caching, possibly using ConditionalWeakTable?

            return newTree.GetChanges(oldTree).Select(c => new TextChangeRange(c.Span, c.NewText.Length)).ToList();
        }
    }
}