using System.Collections.Generic;
using System.Linq;
using CSharpE.Syntax;
using CSharpE.Transform.Internals;

namespace CSharpE.Transform.Transformers
{
    internal sealed class TransformerCollector
    {
        private readonly TransformProject project;
        private readonly IReadOnlyList<CollectionTransformer> oldTransformers;
        private int oldTransformersIndex = 0;

        public TransformerCollector(
            TransformProject project, IReadOnlyList<CollectionTransformer> transformers)
        {
            this.project = project;
            oldTransformers = transformers;
        }

        public List<CollectionTransformer> Transformers { get; } = new List<CollectionTransformer>();

        public TResult Collection<TParent, TItem, TData, TIntermediate, TResult>(
            TParent parent, IEnumerable<TItem> collection,
            ActionInvoker<TData, TItem, TIntermediate, TResult> action, TData data)
            where TParent : class
            where TItem : SyntaxNode
        {
            CollectionTransformer<TParent, TItem, TData, TIntermediate, TResult> transformer = null;

            var oldTransformer = oldTransformers?.ElementAtOrDefault(oldTransformersIndex++);

            if (oldTransformer is CollectionTransformer<TParent, TItem, TData, TIntermediate, TResult> oldCollectionTransformer)
            {
                if (oldCollectionTransformer.Matches(parent, action, data, newLimitedComparison: false))
                    transformer = oldCollectionTransformer;
            }

            if (transformer == null)
                transformer = CollectionTransformer.Create(parent, action, data, limitedComparison: false);

            var result = transformer.Transform(project, collection);

            Transformers.Add(transformer);

            return result;
        }
        
        public TResult LimitedSegment<TNode, TData, TResult>(
            TNode node, ActionInvoker<TData, TNode, TResult, TResult> action, TData data)
            where TNode : SyntaxNode
        {
            // segment is implemented as a single-element collection, because CodeTransformer doesn't handle data
            
            CollectionTransformer<TNode, TNode, TData, TResult, TResult> transformer = null;

            var oldTransformer = oldTransformers?.ElementAtOrDefault(oldTransformersIndex++);

            if (oldTransformer is CollectionTransformer<TNode, TNode, TData, TResult, TResult> oldCollectionTransformer)
            {
                if (oldCollectionTransformer.Matches(node, action, data, newLimitedComparison: true))
                    transformer = oldCollectionTransformer;
            }

            if (transformer == null)
                transformer = CollectionTransformer.Create(node, action, data, limitedComparison: true);

            var result = transformer.Transform(project, new[] { node });

            Transformers.Add(transformer);

            return result;
        }
        
    }
}