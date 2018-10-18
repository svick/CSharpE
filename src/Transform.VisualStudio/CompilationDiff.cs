using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Concurrent;

namespace CSharpE.Transform.VisualStudio
{
    sealed class CompilationDiff
    {
        private readonly CSharpCompilation oldCompilation;
        private readonly CSharpCompilation newCompilation;

        private readonly ConcurrentDictionary<string, SyntaxTreeDiff> cachedDiffs = new ConcurrentDictionary<string, SyntaxTreeDiff>();
        private readonly ConcurrentDictionary<string, SyntaxTreeDiff> cachedReverseDiffs = new ConcurrentDictionary<string, SyntaxTreeDiff>();

        public CompilationDiff(CSharpCompilation oldCompilation, CSharpCompilation newCompilation)
        {
            this.oldCompilation = oldCompilation;
            this.newCompilation = newCompilation;
        }

        private SyntaxTreeDiff BuildTreeDiff(string filePath, bool reverse)
        {
            var oldTree = oldCompilation.GetTreeOrDefault(filePath);
            if (oldTree == null)
                return null;

            var newTree = newCompilation.GetTreeOrDefault(filePath);
            if (newTree == null)
                return null;

            return reverse ? new SyntaxTreeDiff(newTree, oldTree) : new SyntaxTreeDiff(oldTree, newTree);
        }

        private SyntaxTreeDiff ForTree(string filePath, bool reverse)
        {
            var cache = reverse ? cachedReverseDiffs : cachedDiffs;

            return cache.GetOrAdd(filePath, path => BuildTreeDiff(path, reverse));
        }

        internal SyntaxTreeDiff ForTree(string filePath) => ForTree(filePath, reverse: false);

        internal SyntaxTreeDiff ForTreeReverse(string filePath) => ForTree(filePath, reverse: true);
    }
}
