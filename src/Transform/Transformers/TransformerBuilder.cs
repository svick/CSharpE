﻿using System;
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
            TData data) where TParent : class;
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
                transformer = new CollectionTransformer<TParent, TItem, TData>(parent, action, data);

            transformer.Transform(project, collectionFunction(parent));

            Transformers.Add(transformer);
        }
    }

    internal class CollectionTransformer<TParent, TItem, TData> : Transformer<IEnumerable<TItem>>
        where TParent : class
    {
        private static readonly Func<TParent, TParent, bool> ParentMatcher = CreateParentMatcher();

        private static Func<TParent, TParent, bool> CreateParentMatcher()
        {
            if (typeof(SyntaxNode).IsAssignableFrom(typeof(TParent)))
            {
                Func<SyntaxNode, SyntaxNode, bool> syntaxNodeMatcher =
                    (oldNode, newNode) => oldNode.FileSpan.Matches(newNode.FileSpan);

                return (Func<TParent, TParent, bool>)syntaxNodeMatcher;
            }

            if (typeof(Syntax.Project) == typeof(TParent))
            {
                // assume that two instances of Project refer to the same project
                return (oldProject, newProject) => true;
            }

            return ReferenceEquals;
        }

        private readonly TParent parent;
        private readonly ActionInvoker<TData, TItem> action;
        private readonly TData data;

        public CollectionTransformer(TParent parent, ActionInvoker<TData, TItem> action, TData data)
        {
            this.parent = parent;
            this.action = action;
            this.data = data;
        }

        public override void Transform(TransformProject project, IEnumerable<TItem> input)
        {
            foreach (var item in input)
            {
                action.Invoke(data, item);
            }
        }

        public bool Matches(TParent newParent, ActionInvoker<TData, TItem> newAction, TData newData) =>
            action.Equals(newAction) &&
            EqualityComparer<TData>.Default.Equals(data, newData) &&
            ParentMatcher(parent, newParent);
    }
}