using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using CSharpE.Syntax;
using CSharpE.Syntax.Internals;
using CSharpE.Transform.Internals;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using static CSharpE.Syntax.Internals.SpanCompareResult;
using SyntaxNode = CSharpE.Syntax.SyntaxNode;

namespace CSharpE.Transform.Transformers
{
    // transformer for a collection of syntax nodes from the same file
    internal sealed class SyntaxNodeCollectionTransformer<TItem, TData, TIntermediate, TResult>
        : CollectionTransformer<SyntaxNode, TItem, TData, TIntermediate, TResult>
        where TItem : SyntaxNode
    {
        private readonly bool limitedComparison;
        private FileSpan parentFileSpan;

        private SyntaxTree oldTree;
        private List<TextSpan> oldItemsSpans;
        private List<CodeTransformer<TItem, TIntermediate>> oldTransformers;

        public SyntaxNodeCollectionTransformer(
            SyntaxNode parent, ActionInvoker<TData, TItem, TIntermediate, TResult> action, TData data,
            bool limitedComparison)
            : base(action, data)
        {
            this.limitedComparison = limitedComparison;

            parentFileSpan = parent.FileSpan;
        }

        public override TResult Transform(TransformProject project, IEnumerable<TItem> input)
        {
            var items = input.ToList();

            var newFile = items.FirstOrDefault()?.SourceFile;
            var newTree = newFile?.GetSyntaxTree();
            var newItemsSpans = items.Select(x => x.Span).ToList();
            var newTransformers = new List<CodeTransformer<TItem, TIntermediate>>(items.Count);

            var diff = (oldTree != null && newTree != null) ? newTree.GetChangeRanges(oldTree) : null;

            int oldIndex = 0;
            int newIndex = 0;

            while (newIndex < items.Count)
            {
                CodeTransformer<TItem, TIntermediate> itemTransformer = null;

                if (oldIndex < oldItemsSpans?.Count)
                {
                    switch (oldItemsSpans[oldIndex].Compare(newItemsSpans[newIndex], diff))
                    {
                        // not sure what to do in case of Unknown, moving forward in old should be fine
                        case Unknown:
                        case OldFirst:
                            oldIndex++;
                            continue;
                        case Matching:
                            itemTransformer = oldTransformers[oldIndex];
                            oldIndex++;
                            break;
                        // do nothing here; this means new code transformer will be used
                        case NewFirst:
                            break;
                        default:
                            throw new InvalidOperationException("impossible");
                    }
                }

                if (itemTransformer == null)
                    itemTransformer = CodeTransformer<TItem, TIntermediate>.Create(InvokeAndCheck, limitedComparison);

                var newItem = items[newIndex];

                Debug.Assert(ReferenceEquals(newFile, newItem.SourceFile));

                var intermediate = itemTransformer.Transform(project, newItem);
                Action.ProvideIntermediate(intermediate);

                newTransformers.Add(itemTransformer);

                newIndex++;
            }

            oldTree = newTree;
            oldItemsSpans = newItemsSpans;
            oldTransformers = newTransformers;

            return Action.GetResult();
        }

        public override bool Matches(
            SyntaxNode newParent, ActionInvoker<TData, TItem, TIntermediate, TResult> newAction, TData newData,
            bool newLimitedComparison)
        {
            var newParentFileSpan = newParent.FileSpan;

            var result = Action.Equals(newAction) &&
                         GeneralHandler.Equals(Data, newData) &&
                         parentFileSpan.Matches(newParentFileSpan) &&
                         limitedComparison == newLimitedComparison;

            // to make following matches simpler
            if (result)
                parentFileSpan = newParentFileSpan;

            return result;
        }
    }
}
