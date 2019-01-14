using System;
using System.Collections.Generic;
using CSharpE.Syntax;
using CSharpE.Transform.Internals;

namespace CSharpE.Transform.Transformers
{
    internal abstract class CollectionTransformer<TParent, TItem, TData, TIntermediate, TResult>
        : CollectionTransformer
        where TParent : class
        where TItem : SyntaxNode
    {
        protected ActionInvoker<TData, TItem, TIntermediate, TResult> Action { get; }
        protected TData Data { get; }

        protected CollectionTransformer(ActionInvoker<TData, TItem, TIntermediate, TResult> action, TData data)
        {
            Action = action;
            Data = GeneralHandler.DeepClone(data);
        }

        public abstract TResult Transform(TransformProject project, IEnumerable<TItem> input);

        public abstract bool Matches(TParent newParent, ActionInvoker<TData, TItem, TIntermediate, TResult> newAction,
            TData newData, bool newLimitedComparison);

        protected TIntermediate InvokeAndCheck(TItem item)
        {
            if (GeneralHandler.IsImmutable(Data))
                return Action.Invoke(Data, item);

            var oldData = GeneralHandler.DeepClone(Data);
            var result = Action.Invoke(Data, item);

            if (!GeneralHandler.Equals(oldData, Data))
                throw new InvalidOperationException(
                    "It is not allowed to mutate inputs of smart methods in their bodies.");

            return result;
        }
    }

    internal abstract class CollectionTransformer
    {
        public static CollectionTransformer<TParent, TItem, TData, TIntermediate, TOutput> Create<TParent, TItem, TData, TIntermediate, TOutput>(
            TParent parent, ActionInvoker<TData, TItem, TIntermediate, TOutput> action, TData data,
            bool limitedComparison)
            where TParent : class
            where TItem : SyntaxNode
        {
            object result;

            if (typeof(TParent) == typeof(Project) && typeof(TItem) == typeof(SourceFile))
            {
                result = new SourceFileCollectionTransformer<TData, TIntermediate, TOutput>(
                    (Project)(object)parent,
                    (ActionInvoker<TData, SourceFile, TIntermediate, TOutput>)(object)action, data);
            }
            else if (typeof(TParent) == typeof(SyntaxNode))
            {
                result = new SyntaxNodeCollectionTransformer<TItem, TData, TIntermediate, TOutput>(
                    (SyntaxNode)(object)parent, action, data, limitedComparison);
            }
            else
                throw new InvalidOperationException();

            return (CollectionTransformer<TParent, TItem, TData, TIntermediate, TOutput>)result;
        }
    }
}