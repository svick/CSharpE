using System;
using System.Collections.Generic;
using CSharpE.Syntax;
using CSharpE.Transform.Internals;

namespace CSharpE.Transform.Transformers
{
    internal abstract class CollectionTransformer<TParent, TItem, TData> : Transformer<IEnumerable<TItem>>
        where TParent : class
        where TItem : SyntaxNode
    {
        protected ActionInvoker<TData, TItem> Action { get; }
        protected TData Data { get; }

        protected CollectionTransformer(ActionInvoker<TData, TItem> action, TData data)
        {
            Action = action;
            Data = data;
        }

        public abstract bool Matches(TParent newParent, ActionInvoker<TData, TItem> newAction, TData newData);
    }

    internal static class CollectionTransformer
    {
        public static CollectionTransformer<TParent, TItem, TData> Create<TParent, TItem, TData>(
            TParent parent, ActionInvoker<TData, TItem> action, TData data)
            where TParent : class
            where TItem : SyntaxNode
        {
            Type transfomerType;

            // PERF: SourceFileCollectionTransformer doesn't need reflection; SyntaxNodeCollectionTransformer might profit from ExpressionTree-based cache

            if (typeof(TParent) == typeof(Syntax.Project) && typeof(TItem) == typeof(Syntax.SourceFile))
                transfomerType = typeof(SourceFileCollectionTransformer<TData>);
            else if (typeof(SyntaxNode).IsAssignableFrom(typeof(TParent)))
                transfomerType = typeof(SyntaxNodeCollectionTransformer<,,>)
                    .MakeGenericType(typeof(TParent), typeof(TItem), typeof(TData));
            else
                throw new InvalidOperationException();

            return (CollectionTransformer<TParent, TItem, TData>)Activator.CreateInstance(
                transfomerType, parent, action, data);
        }
    }
}