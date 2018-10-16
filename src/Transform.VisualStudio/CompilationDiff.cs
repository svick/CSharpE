using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Generic;
using System.Linq;
using RoslynSyntaxTree = Microsoft.CodeAnalysis.SyntaxTree;

namespace CSharpE.Transform.VisualStudio
{
    sealed class CompilationDiff
    {
        private readonly CSharpCompilation oldCompilation;
        private readonly CSharpCompilation newCompilation;

        private readonly Dictionary<string, SyntaxTreeDiff> cachedDiffs = new Dictionary<string, SyntaxTreeDiff>();
        private readonly Dictionary<string, SyntaxTreeDiff> cachedReverseDiffs = new Dictionary<string, SyntaxTreeDiff>();

        public CompilationDiff(CSharpCompilation oldCompilation, CSharpCompilation newCompilation)
        {
            this.oldCompilation = oldCompilation;
            this.newCompilation = newCompilation;
        }

        private SyntaxTreeDiff BuildTreeDiff(string filePath, bool reverse)
        {
            RoslynSyntaxTree GetTree(CSharpCompilation compilation)
            {
                var trees = compilation.SyntaxTrees.Where(tree => tree.FilePath == filePath).ToList();
                if (trees.Count != 1)
                    return null;
                return trees[0];
            }

            var oldTree = GetTree(oldCompilation);
            if (oldTree == null)
                return null;

            var newTree = GetTree(newCompilation);
            if (newTree == null)
                return null;

            return reverse ? new SyntaxTreeDiff(newTree, oldTree) : new SyntaxTreeDiff(oldTree, newTree);
        }

        private SyntaxTreeDiff ForTree(string filePath, bool reverse)
        {
            var cache = reverse ? cachedReverseDiffs : cachedDiffs;

            if (!cache.TryGetValue(filePath, out var diff))
            {
                diff = BuildTreeDiff(filePath, reverse);
                cache[filePath] = diff;
            }
            return diff;
        }

        internal SyntaxTreeDiff ForTree(string filePath) => ForTree(filePath, reverse: false);

        internal SyntaxTreeDiff ForTreeReverse(string filePath) => ForTree(filePath, reverse: true);
    }
}
