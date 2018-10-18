using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Linq;
using System.Text;
using RoslynSyntaxTree = Microsoft.CodeAnalysis.SyntaxTree;

namespace CSharpE.Transform.VisualStudio
{
    internal static class RoslynExtensions
    {
        public static RoslynSyntaxTree GetTree(this CSharpCompilation compilation, string filePath)
            => compilation.SyntaxTrees.Single(tree => tree.FilePath == filePath);

        public static RoslynSyntaxTree GetTreeOrDefault(this CSharpCompilation compilation, string filePath)
        {
            // PERF: unnecessary List allocation
            var trees = compilation.SyntaxTrees.Where(tree => tree.FilePath == filePath).ToList();
            if (trees.Count != 1)
                return null;
            return trees[0];
        }

        // https://stackoverflow.com/a/27106959/41071
        public static string GetFullMetadataName(this ISymbol symbol)
        {
            if (symbol == null || IsRootNamespace(symbol))
            {
                return string.Empty;
            }

            var sb = new StringBuilder(symbol.MetadataName);
            var last = symbol;

            symbol = symbol.ContainingSymbol;

            while (!IsRootNamespace(symbol))
            {
                if (symbol is ITypeSymbol && last is ITypeSymbol)
                {
                    sb.Insert(0, '+');
                }
                else
                {
                    sb.Insert(0, '.');
                }

                sb.Insert(0, symbol.MetadataName);
                symbol = symbol.ContainingSymbol;
            }

            return sb.ToString();
        }

        private static bool IsRootNamespace(ISymbol symbol) => symbol is INamespaceSymbol s && s.IsGlobalNamespace;
    }
}
