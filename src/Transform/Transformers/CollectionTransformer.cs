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

        public abstract bool Matches(TParent newParent, ActionInvoker<TData, TItem, TIntermediate, TResult> newAction,
            TData newData, bool newLimitedComparison);
    }

    internal static class CollectionTransformer
    {
        public static CollectionTransformer<TParent, TItem, TData, TIntermediate, TOutput> Create<TParent, TItem, TData, TIntermediate, TOutput>(
            TParent parent, ActionInvoker<TData, TItem, TIntermediate, TOutput> action, TData data,
            bool limitedComparison)
            where TParent : class
            where TItem : SyntaxNode
        {
            // SyntaxNodeCollectionTransformer might profit from ExpressionTree-based cache

            object result;

            if (typeof(TParent) == typeof(Syntax.Project) && typeof(TItem) == typeof(Syntax.SourceFile))
            {
                result = new SourceFileCollectionTransformer<TData, TIntermediate, TOutput>(
                    (Syntax.Project)(object)parent,
                    (ActionInvoker<TData, Syntax.SourceFile, TIntermediate, TOutput>)(object)action, data);
            }
            else if (typeof(SyntaxNode).IsAssignableFrom(typeof(TParent)))
            {
                var transfomerType = typeof(SyntaxNodeCollectionTransformer<,,,,>).MakeGenericType(
                    typeof(TParent), typeof(TItem), typeof(TData), typeof(TIntermediate), typeof(TOutput));
                result = Activator.CreateInstance(transfomerType, parent, action, data, limitedComparison);
            }
            else
                throw new InvalidOperationException();

            return (CollectionTransformer<TParent, TItem, TData, TIntermediate, TOutput>)result;
        }
    }
}