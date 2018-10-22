using System;
using System.Linq;
using System.Text;
using System.Threading;
using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using RoslynSyntaxTree = Microsoft.CodeAnalysis.SyntaxTree;

namespace CSharpE.Transform.VisualStudio
{
    internal sealed class SyntaxTree : CSharpSyntaxTree
    {
        private readonly CSharpSyntaxTree roslynTree;

        public SyntaxTree(CSharpSyntaxTree roslynTree) => this.roslynTree = Annotate(roslynTree);

        internal static SyntaxNode Annotate(SyntaxNode node)
        {
            bool NeedsAnnotation(SyntaxNode n)
            {
                switch (n.Kind())
                {
                    // TODO: list all relevant node kinds
                    case SyntaxKind.ClassDeclaration:
                    case SyntaxKind.StructDeclaration:
                    case SyntaxKind.InterfaceDeclaration:
                    case SyntaxKind.EnumDeclaration:
                    case SyntaxKind.MethodDeclaration:
                        return Annotation.Get(node) == null;

                    default:
                        return false;
                }
            }

            return node.ReplaceNodes(
                node.DescendantNodes().Where(NeedsAnnotation),
                (_, n) => n.WithAdditionalAnnotations(Annotation.Create()));
        }

        private static TTree Annotate<TTree>(TTree roslynSyntaxTree) where TTree : RoslynSyntaxTree =>
            (TTree)roslynSyntaxTree.WithRootAndOptions(Annotate(roslynSyntaxTree.GetRoot()), roslynSyntaxTree.Options);

        public override bool TryGetText(out SourceText text)
        {
            return roslynTree.TryGetText(out text);
        }

        public override SourceText GetText(CancellationToken cancellationToken = new CancellationToken())
        {
            return roslynTree.GetText(cancellationToken);
        }

        public override bool TryGetRoot(out CSharpSyntaxNode root)
        {
            return roslynTree.TryGetRoot(out root);
        }

        public override CSharpSyntaxNode GetRoot(CancellationToken cancellationToken)
        {
            return roslynTree.GetRoot(cancellationToken);
        }

        /*
        public override RoslynSyntaxTree WithChangedText(SourceText newText)
        {
            return Wrap(RoslynTree.WithChangedText(newText));
        }
        */

        public override SyntaxReference GetReference(SyntaxNode node)
        {
            return roslynTree.GetReference(node);
        }

        public override RoslynSyntaxTree WithRootAndOptions(SyntaxNode root, ParseOptions options)
        {
            throw new NotImplementedException();
            return roslynTree.WithRootAndOptions(root, options);
        }

        public override RoslynSyntaxTree WithFilePath(string path)
        {
            return roslynTree.WithFilePath(path);
        }

        public override string FilePath => roslynTree.FilePath;

        public override bool HasCompilationUnitRoot => roslynTree.HasCompilationUnitRoot;

        public override CSharpParseOptions Options => roslynTree.Options;

        public override int Length => roslynTree.Length;

        public override Encoding Encoding => roslynTree.Encoding;
    }
}
