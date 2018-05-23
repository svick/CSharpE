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
    internal class SyntaxNodeCollectionTransformer<TParent, TItem, TData> : CollectionTransformer<TParent, TItem, TData>
        where TParent : SyntaxNode
        where TItem : SyntaxNode
    {
        private readonly FileSpan parentFileSpan;
        private readonly ActionInvoker<TData, TItem> action;
        private readonly TData data;

        private SyntaxTree oldTree;
        private List<TextSpan> oldItemsSpans;
        private List<CodeTransformer<TItem>> oldTransformers;

        public SyntaxNodeCollectionTransformer(TParent parent, ActionInvoker<TData, TItem> action, TData data)
        {
            this.parentFileSpan = parent.FileSpan;
            this.action = action;
            this.data = data;
        }

        public override void Transform(TransformProject project, IEnumerable<TItem> input)
        {
            var items = input.ToList();

            var newFile = items.FirstOrDefault()?.SourceFile;
            var newTree = newFile?.GetWrapped();
            var newItemsSpans = items.Select(x => x.Span).ToList();
            var newTransformers = new List<CodeTransformer<TItem>>(items.Count);

            var diff = (oldTree != null && newTree != null) ? newTree.GetChangeRanges(oldTree) : null;

            int oldIndex = 0;
            int newIndex = 0;

            while (newIndex < items.Count)
            {
                CodeTransformer<TItem> itemTransformer = null;

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
                    itemTransformer = new CodeTransformer<TItem>(i => action.Invoke(data, i));

                var newItem = items[newIndex];

                Debug.Assert(ReferenceEquals(newFile, newItem.SourceFile));

                itemTransformer.Transform(project, newItem);

                newTransformers.Add(itemTransformer);

                newIndex++;
            }

            oldTree = newTree;
            oldItemsSpans = newItemsSpans;
            oldTransformers = newTransformers;
        }

        public override bool Matches(TParent newParent, ActionInvoker<TData, TItem> newAction, TData newData) =>
            action.Equals(newAction) &&
            EqualityComparer<TData>.Default.Equals(data, newData) &&
            parentFileSpan.Matches(newParent.FileSpan);
    }
}