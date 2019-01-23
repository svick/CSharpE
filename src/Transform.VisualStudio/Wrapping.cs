using Microsoft.CodeAnalysis.CSharp;
using RoslynCompilation = Microsoft.CodeAnalysis.Compilation;
using RoslynSyntaxTree = Microsoft.CodeAnalysis.SyntaxTree;

namespace CSharpE.Transform.VisualStudio
{
    internal static class Wrapping
    {
        // TODO: delete
        public static RoslynSyntaxTree Unwrap(RoslynSyntaxTree tree) => tree;// ((SyntaxTree)tree).RoslynTree;

        public static Compilation Wrap(RoslynCompilation roslynCompilation) => new Compilation((CSharpCompilation)roslynCompilation);
    }
}
