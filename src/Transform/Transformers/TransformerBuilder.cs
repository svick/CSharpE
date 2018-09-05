using System;
using System.Collections.Generic;
using System.Linq;
using CSharpE.Syntax;
using CSharpE.Transform.Internals;

namespace CSharpE.Transform.Transformers
{
    internal class TransformerBuilder
    {
        private readonly TransformProject project;
        private readonly IReadOnlyList<Transformer> oldTransformers;
        private int oldTransformersIndex = 0;

        public TransformerBuilder(
            TransformProject project, IReadOnlyList<Transformer> transformers)
        {
            this.project = project;
            oldTransformers = transformers;
        }

        public List<Transformer> Transformers { get; } = new List<Transformer>();

        public TResult Collection<TParent, TItem, TData, TIntermediate, TResult>(
            TParent parent, Func<TParent, IEnumerable<TItem>> collectionFunction,
            ActionInvoker<TData, TItem, TIntermediate, TResult> action, TData data)
            where TParent : class
            where TItem : SyntaxNode
        {
            CollectionTransformer<TParent, TItem, TData, TIntermediate, TResult> transformer = null;

            var oldTransformer = oldTransformers?.ElementAtOrDefault(oldTransformersIndex++);

            if (oldTransformer is CollectionTransformer<TParent, TItem, TData, TIntermediate, TResult> oldCollectionTransformer)
            {
                if (oldCollectionTransformer.Matches(parent, action, data))
                    transformer = oldCollectionTransformer;
            }

            if (transformer == null)
                transformer = CollectionTransformer.Create(parent, action, data);

            var result = transformer.Transform(project, collectionFunction(parent));

            Transformers.Add(transformer);

            return result;
        }
    }
}