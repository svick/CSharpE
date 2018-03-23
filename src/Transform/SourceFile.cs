using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace CSharpE.Transform
{
    public class SourceFile
    {
        public string Path { get; }

        public SyntaxTree Tree { get; set; }

        private SyntaxTree treeSnapshot;

        public string Text => Tree.ToString();

        public SourceFile(string path, string text)
            : this(path, CSharpSyntaxTree.ParseText(text)) { }

        private SourceFile(string path, SyntaxTree tree)
        {
            Path = path;
            Tree = tree;
        }

        internal Syntax.SourceFile ToSyntaxSourceFile() => new Syntax.SourceFile(Path, Tree);

        internal static SourceFile FromSyntaxSourceFile(Syntax.SourceFile syntaxSourceFile) =>
            new SourceFile(syntaxSourceFile.Path, syntaxSourceFile.GetWrapped());

        internal FileDiff Diff() => FileDiff.Between(treeSnapshot, Tree);

        internal void Snapshot() => treeSnapshot = Tree;
    }
}