using RoslynCompilation = Microsoft.CodeAnalysis.Compilation;
using RoslynSyntaxTree = Microsoft.CodeAnalysis.SyntaxTree;

namespace CSharpE.Transform.VisualStudio
{
    internal static class Wrapping
    {
        public static RoslynSyntaxTree Wrap(RoslynSyntaxTree roslynTree) => roslynTree;// new SyntaxTree(roslynTree);
        public static RoslynSyntaxTree Unwrap(RoslynSyntaxTree tree) => tree;// ((SyntaxTree)tree).RoslynTree;

        public static Compilation Wrap(RoslynCompilation roslynCompilation) => new Compilation(roslynCompilation);
    }
}
