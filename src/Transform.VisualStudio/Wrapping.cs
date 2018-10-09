using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RoslynCompilation = Microsoft.CodeAnalysis.Compilation;
using RoslynSyntaxTree = Microsoft.CodeAnalysis.SyntaxTree;

namespace CSharpE.Transform.VisualStudio
{
    internal static class Wrapping
    {
        public static SyntaxTree Wrap(RoslynSyntaxTree roslynTree) => new SyntaxTree(roslynTree);
        public static RoslynSyntaxTree Unwrap(RoslynSyntaxTree tree) => ((SyntaxTree)tree).RoslynTree;

        public static Compilation Wrap(RoslynCompilation roslynCompilation) => new Compilation(roslynCompilation);
    }
}
