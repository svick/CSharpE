using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace CSharpE.Syntax
{
    public class SourceFile
    {
        public string Path { get; }

        private CompilationUnitSyntax compilationUnit;

        public string GetText() => compilationUnit.ToFullString();
        
        public bool IsModified => throw new NotImplementedException();

        private SourceFile(string path, CompilationUnitSyntax compilationUnit)
        {
            Path = path;
            this.compilationUnit = compilationUnit;
        }

        public SourceFile(string path, string text)
        {
            Path = path;
            compilationUnit = SyntaxFactory.ParseCompilationUnit(text);
        }

        public static async Task<SourceFile> OpenAsync(string path)
        {
            using (var stream = File.OpenRead(path))
            {
                // there is no async version of SourceText.From: https://github.com/dotnet/roslyn/issues/20796
                var compilationUnit = CSharpSyntaxTree.ParseText(await Task.Run(() => SourceText.From(stream))).GetCompilationUnitRoot();
                
                return new SourceFile(path, compilationUnit);
            }
        }
    }
}