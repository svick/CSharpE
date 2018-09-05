using System;
using System.Collections.Generic;
using CSharpE.Syntax;
using CSharpE.Transform.Internals;

namespace CSharpE.Transform.Transformers
{
    internal abstract class CollectionTransformer<TParent, TItem, TData, TIntermediate, TResult>
        : Transformer<IEnumerable<TItem>, TResult>
        where TParent : class
        where TItem : SyntaxNode
    {
        protected ActionInvoker<TData, TItem, TIntermediate, TResult> Action { get; }
        protected TData Data { get; }

        protected CollectionTransformer(ActionInvoker<TData, TItem, TIntermediate, TResult> action, TData data)
        {
            Action = action;
            Data = data;
        }

        public abstract bool Matches(
            TParent newParent, ActionInvoker<TData, TItem, TIntermediate, TResult> newAction, TData newData);
    }

    internal static class CollectionTransformer
    {
        public static CollectionTransformer<TParent, TItem, TData, TIntermediate, TOutput> Create<TParent, TItem, TData, TIntermediate, TOutput>(
            TParent parent, ActionInvoker<TData, TItem, TIntermediate, TOutput> action, TData data)
            where TParent : class
            where TItem : SyntaxNode
        {
            Type transfomerType;

            // PERF: SourceFileCollectionTransformer doesn't need reflection
            // SyntaxNodeCollectionTransformer might profit from ExpressionTree-based cache

            if (typeof(TParent) == typeof(Syntax.Project) && typeof(TItem) == typeof(Syntax.SourceFile))
                transfomerType = typeof(SourceFileCollectionTransformer<TData, TIntermediate, TOutput>);
            else if (typeof(SyntaxNode).IsAssignableFrom(typeof(TParent)))
                transfomerType = typeof(SyntaxNodeCollectionTransformer<,,,,>).MakeGenericType(
                    typeof(TParent), typeof(TItem), typeof(TData), typeof(TIntermediate), typeof(TOutput));
            else
                throw new InvalidOperationException();

            return (CollectionTransformer<TParent, TItem, TData, TIntermediate, TOutput>)Activator.CreateInstance(
                transfomerType, parent, action, data);
        }
    }
}