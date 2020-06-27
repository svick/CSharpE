using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using RoslynSyntaxTree = Microsoft.CodeAnalysis.SyntaxTree;

namespace CSharpE.Transform.VisualStudio
{
    public class SyntaxDiff
    {
        private readonly SyntaxNode oldNode;
        private readonly SyntaxNode newNode;

        protected Lazy<IList<TextChange>> Changes { get; }

        protected virtual IList<TextChange> ComputeChanges() => SyntaxDiffer.GetTextChanges(oldNode, newNode);

        public SyntaxDiff(SyntaxNode oldNode, SyntaxNode newNode)
        {
            this.oldNode = oldNode;
            this.newNode = newNode;

            Changes = new Lazy<IList<TextChange>>(ComputeChanges);
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

        public TextSpan AdjustLoose(TextSpan span)
        {
            int start = AdjustLoose(span.Start);

            int end = AdjustLoose(span.End);

            return TextSpan.FromBounds(start, end);
        }

        public int? Adjust(int position)
        {
            int? result = position;

            foreach (var change in Changes.Value)
            {
                result = Adjust(position, result, change);
            }

            return result;
        }

        private static int? Adjust(int originalPosition, int? position, TextChangeRange change, bool loose = false)
        {
            if (position == null)
                return null;

            var pos = position.Value;

            int diff = change.NewLength - change.Span.Length;

            if (change.Span.End <= originalPosition)
                return pos + diff;

            if (change.Span.Start > originalPosition)
                return pos;

            if (loose)
                return pos - (originalPosition - change.Span.Start);

            return null;
        }

        public int AdjustLoose(int position)
        {
            int? result = position;

            foreach (var change in Changes.Value)
            {
                result = Adjust(position, result, change, loose: true);
            }

            return result.Value;
        }
    }

    public class SyntaxTreeDiff : SyntaxDiff
    {
        private readonly RoslynSyntaxTree oldTree;
        private readonly RoslynSyntaxTree newTree;

        protected override IList<TextChange> ComputeChanges() => SyntaxDiffer.GetTextChanges(oldTree, newTree);

        public SyntaxTreeDiff(RoslynSyntaxTree oldTree, RoslynSyntaxTree newTree)
            : base(oldTree.GetRoot(), newTree.GetRoot())
        {
            this.oldTree = oldTree;
            this.newTree = newTree;
        }

        public Diagnostic Adjust(Diagnostic diagnostic) => diagnostic.WithLocation(AdjustLoose(diagnostic.Location));

        private (SyntaxNode oldNode, SyntaxNode newNode)? FindNodePair(SyntaxNode oldNode)
        {
            while (oldNode != null)
            {
                var annotation = Annotation.Get(oldNode);
                if (annotation != null)
                {
                    var newNode = newTree.GetRoot().GetAnnotatedNodes(annotation).FirstOrDefault();
                    if (newNode != null)
                    {
                        return (oldNode, newNode);
                    }
                }

                oldNode = oldNode.Parent;
            }

            return null;
        }

        private Location AdjustLoose(Location location)
        {
            if (location.Kind != LocationKind.SourceFile)
                return location;

            Debug.Assert(location.SourceTree == oldTree);

            var oldNode = location.SourceTree.GetRoot().FindNode(location.SourceSpan);
            var nodePair = FindNodePair(oldNode);

            TextSpan adjusted;
            if (nodePair != null)
            {
                var (foundOldNode, foundNewNode) = nodePair.Value;

                if (location == foundOldNode.GetLocation())
                    return foundNewNode.GetLocation();

                var nodeDiff = new SyntaxDiff(foundOldNode, foundNewNode);
                var locallyAdjusted = nodeDiff.AdjustLoose(location.SourceSpan);
                var globalChange = foundNewNode.GetLocation().SourceSpan.Start - foundOldNode.GetLocation().SourceSpan.Start;
                adjusted = TextSpan.FromBounds(locallyAdjusted.Start + globalChange, locallyAdjusted.End + globalChange);
            }
            else
            {
                adjusted = AdjustLoose(location.SourceSpan);
            }

            return Location.Create(newTree, adjusted);
        }
    }
}