using CSharpE.Syntax;
using Microsoft.CodeAnalysis;

namespace CSharpE.Transform.Execution
{
    public static class SourceFileExtensions
    {
        public static SyntaxTree GetSyntaxTree(this SourceFile sourceFile) => sourceFile.GetSyntaxTree();
    }
}
