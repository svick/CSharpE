using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using RoslynSyntaxTree = Microsoft.CodeAnalysis.SyntaxTree;
using static CSharpE.Transform.VisualStudio.Wrapping;

namespace CSharpE.Transform.VisualStudio
{
    internal sealed class SyntaxTree : RoslynSyntaxTree
    {
        public RoslynSyntaxTree RoslynTree { get; }

        public SyntaxTree(RoslynSyntaxTree roslynTree) => RoslynTree = roslynTree;

        public override bool TryGetText(out SourceText text)
        {
            return RoslynTree.TryGetText(out text);
        }

        public override SourceText GetText(CancellationToken cancellationToken = new CancellationToken())
        {
            return RoslynTree.GetText(cancellationToken);
        }

        protected override bool TryGetRootCore(out SyntaxNode root)
        {
            return RoslynTree.TryGetRoot(out root);
        }

        protected override SyntaxNode GetRootCore(CancellationToken cancellationToken)
        {
            return RoslynTree.GetRoot(cancellationToken);
        }

        protected override Task<SyntaxNode> GetRootAsyncCore(CancellationToken cancellationToken)
        {
            return RoslynTree.GetRootAsync(cancellationToken);
        }

        public override RoslynSyntaxTree WithChangedText(SourceText newText)
        {
            return Wrap(RoslynTree.WithChangedText(newText));
        }

        public override IEnumerable<Diagnostic> GetDiagnostics(CancellationToken cancellationToken = new CancellationToken())
        {
            //return roslynTree.GetDiagnostics(cancellationToken);
            return Enumerable.Empty<Diagnostic>();
        }

        public override IEnumerable<Diagnostic> GetDiagnostics(SyntaxNode node)
        {
            return RoslynTree.GetDiagnostics(node);
        }

        public override IEnumerable<Diagnostic> GetDiagnostics(SyntaxToken token)
        {
            return RoslynTree.GetDiagnostics(token);
        }

        public override IEnumerable<Diagnostic> GetDiagnostics(SyntaxTrivia trivia)
        {
            return RoslynTree.GetDiagnostics(trivia);
        }

        public override IEnumerable<Diagnostic> GetDiagnostics(SyntaxNodeOrToken nodeOrToken)
        {
            return RoslynTree.GetDiagnostics(nodeOrToken);
        }

        public override FileLinePositionSpan GetLineSpan(TextSpan span, CancellationToken cancellationToken = new CancellationToken())
        {
            return RoslynTree.GetLineSpan(span, cancellationToken);
        }

        public override FileLinePositionSpan GetMappedLineSpan(TextSpan span, CancellationToken cancellationToken = new CancellationToken())
        {
            return RoslynTree.GetMappedLineSpan(span, cancellationToken);
        }

        public override bool HasHiddenRegions()
        {
            return RoslynTree.HasHiddenRegions();
        }

        public override IList<TextSpan> GetChangedSpans(RoslynSyntaxTree syntaxTree)
        {
            return RoslynTree.GetChangedSpans(syntaxTree);
        }

        public override Location GetLocation(TextSpan span)
        {
            return RoslynTree.GetLocation(span);
        }

        public override bool IsEquivalentTo(RoslynSyntaxTree tree, bool topLevel = false)
        {
            return RoslynTree.IsEquivalentTo(tree, topLevel);
        }

        public override SyntaxReference GetReference(SyntaxNode node)
        {
            return RoslynTree.GetReference(node);
        }

        public override IList<TextChange> GetChanges(RoslynSyntaxTree oldTree)
        {
            return RoslynTree.GetChanges(oldTree);
        }

        public override RoslynSyntaxTree WithRootAndOptions(SyntaxNode root, ParseOptions options)
        {
            throw new NotImplementedException();
            return RoslynTree.WithRootAndOptions(root, options);
        }

        public override RoslynSyntaxTree WithFilePath(string path)
        {
            throw new NotImplementedException();
            return RoslynTree.WithFilePath(path);
        }

        public override string FilePath => RoslynTree.FilePath;

        public override bool HasCompilationUnitRoot => RoslynTree.HasCompilationUnitRoot;

        protected override ParseOptions OptionsCore => RoslynTree.Options;

        public override int Length => RoslynTree.Length;

        public override Encoding Encoding => RoslynTree.Encoding;
    }
}
