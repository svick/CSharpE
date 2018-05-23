using System;
using System.Collections.Generic;
using System.Linq;
using CSharpE.Syntax;
using CSharpE.Transform.Internals;

namespace CSharpE.Transform.Transformers
{
    internal abstract class TransformerBuilder
    {
        public abstract void Collection<TParent, TItem, TData>(
            TParent parent, Func<TParent, IEnumerable<TItem>> collectionFunction, ActionInvoker<TData, TItem> action,
            TData data)
            where TParent : class
            where TItem : SyntaxNode;
    }

    internal class TransformerBuilder<TInput> : TransformerBuilder
    {
        private readonly TransformProject project;
        private readonly TInput input;
        private readonly IReadOnlyList<Transformer> oldTransformers;
        private int oldTransformersIndex = 0;

        public TransformerBuilder(
            TransformProject project, TInput input, IReadOnlyList<Transformer> transformers)
        {
            this.project = project;
            this.input = input;
            oldTransformers = transformers;
        }

        public List<Transformer> Transformers { get; } = new List<Transformer>();

        public override void Collection<TParent, TItem, TData>(
            TParent parent, Func<TParent, IEnumerable<TItem>> collectionFunction, ActionInvoker<TData, TItem> action,
            TData data)
        {
            CollectionTransformer<TParent, TItem, TData> transformer = null;

            var oldTransformer = oldTransformers?.ElementAtOrDefault(oldTransformersIndex++);

            if (oldTransformer is CollectionTransformer<TParent, TItem, TData> oldCollectionTransformer)
            {
                if (oldCollectionTransformer.Matches(parent, action, data))
                    transformer = oldCollectionTransformer;
            }

            if (transformer == null)
                transformer = CollectionTransformer.Create(parent, action, data);

            transformer.Transform(project, collectionFunction(parent));

            Transformers.Add(transformer);
        }
    }
}