using System;
using System.Collections.Generic;
using CSharpE.Syntax;
using CSharpE.Transform.Internals;

namespace CSharpE.Transform.Transformers
{
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