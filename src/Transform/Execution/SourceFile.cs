using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace CSharpE.Transform.Execution
{
    public class SourceFile
    {
        public string Path { get; }

        public SyntaxTree Tree { get; set; }

        public string Text => Tree.ToString();

        public SourceFile(string path, string text)
            : this(path, CSharpSyntaxTree.ParseText(text)) { }

        public SourceFile(string path, SyntaxTree tree)
        {
            Path = path;
            Tree = tree;
        }

        public SourceFile(SyntaxTree tree)
            : this(tree.FilePath, tree) { }

        internal Syntax.SourceFile ToSyntaxSourceFile() => new Syntax.SourceFile(Path, Tree);

        public static SourceFile FromSyntaxSourceFile(Syntax.SourceFile syntaxSourceFile) =>
            new SourceFile(syntaxSourceFile.Path, syntaxSourceFile.GetSyntaxTree());

        public static async Task<SourceFile> OpenAsync(string path) =>
            FromSyntaxSourceFile(await Syntax.SourceFile.OpenAsync(path));

        public async Task ReopenAsync()
        {
            var syntaxFile = await Syntax.SourceFile.OpenAsync(Path);

            Tree = syntaxFile.GetSyntaxTree();
        }
    }
}