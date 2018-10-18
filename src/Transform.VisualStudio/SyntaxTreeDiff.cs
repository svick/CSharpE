using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using RoslynSyntaxTree = Microsoft.CodeAnalysis.SyntaxTree;

namespace CSharpE.Transform.VisualStudio
{
    internal class SyntaxTreeDiff
    {
        private readonly RoslynSyntaxTree oldTree;
        private readonly RoslynSyntaxTree newTree;

        private readonly IList<TextChange> changes;

        public SyntaxTreeDiff(RoslynSyntaxTree oldTree, RoslynSyntaxTree newTree)
        {
            this.oldTree = oldTree;
            this.newTree = newTree;

            changes = SyntaxDiffer.GetTextChanges(oldTree, newTree);
        }

        public Diagnostic Adjust(Diagnostic diagnostic) => diagnostic.WithLocation(Adjust(diagnostic.Location));

        public Location Adjust(Location location)
        {
            if (location.Kind != LocationKind.SourceFile)
                return location;

            Debug.Assert(location.SourceTree == oldTree);

            var adjusted = Adjust(location.SourceSpan);

            if (adjusted == null)
                return Location.None;

            return Location.Create(newTree, adjusted.Value);
        }

        public TextSpan? Adjust(TextSpan span)
        {
            int? start = Adjust(span.Start);
            if (start == null)
                return null;

            int? end = Adjust(span.End);
            if (end == null)
                return null;

            return TextSpan.FromBounds(start.Value, end.Value);
        }

        public int? Adjust(int position)
        {
            int? result = position;

            foreach (var change in changes)
            {
                result = Adjust(result, change);
            }

            return result;
        }

        private static int? Adjust(int? position, TextChangeRange change)
        {
            if (position == null)
                return null;

            var pos = position.Value;

            int diff = change.NewLength - change.Span.Length;

            if (change.Span.End < pos)
                return pos + diff;

            if (change.Span.Start > pos)
                return pos;

            // TODO?
            return null;
        }

        internal TNode Adjust<TNode>(TNode syntaxNode) where TNode : SyntaxNode
        {
            var newSpan = Adjust(syntaxNode.Span);

            if (newSpan == null)
                return null;

            return newTree.GetRoot().FindNode(newSpan.Value) as TNode;
        }
    }
}