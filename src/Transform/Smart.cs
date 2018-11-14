using System;
using System.Collections.Generic;
using CSharpE.Syntax;
using CSharpE.Syntax.Internals;
using CSharpE.Transform.Internals;

namespace CSharpE.Transform
{
    public static class Smart
    {
        private class Visitor<TNode, TArg> : ISyntaxCollectionVisitor<TNode> where TNode : SyntaxNode
        {
            private readonly TArg arg;
            private readonly Action<TArg, TNode> action;

            public Visitor(TArg arg, Action<TArg, TNode> action)
            {
                this.arg = arg;
                this.action = action;
            }

            public void Visit<TParent, TChild>(
                Syntax.Project project, TParent parent, IEnumerable<TChild> children,
                Func<TChild, IEnumerable<TNode>> itemFunction)
                where TParent : class
                where TChild : SyntaxNode =>
                Visit(
                    project, parent, children, (arg, action, itemFunction),
                    (t, child) => ForEach(t.itemFunction(child), t.arg, t.action));

            public void Visit<TParent>(Syntax.Project project, TParent parent, IEnumerable<TNode> items)
                where TParent : class =>
                Visit(project, parent, items, arg, action);

            private static void Visit<TParent, TChild, TVisitArg>(
                Syntax.Project project, TParent parent, IEnumerable<TChild> children, TVisitArg visitArg,
                Action<TVisitArg, TChild> childAction)
                where TParent : class
                where TChild : SyntaxNode
            {
                if (project is TransformProject transformProject)
                {
                    transformProject.TransformerBuilder.Collection(
                        parent, children, ActionInvoker.Create(childAction), visitArg);
                }
                else
                {
                    foreach (var item in children)
                    {
                        childAction(visitArg, item);
                    }
                }
            }
        }

        private class Visitor<TNode, TArg, TIntermediate, TResult> : ISyntaxCollectionVisitor<TNode>
            where TNode : SyntaxNode
        {
            private readonly TArg arg;
            private readonly Func<TArg, TNode, TIntermediate> action;
            private readonly Func<TResult, TIntermediate, TResult> simpleCombine;
            private readonly Func<TResult, TResult, TResult> complexCombine;

            public TResult Result { get; private set; }

            public Visitor(
                TArg arg, Func<TArg, TNode, TIntermediate> action,
                Func<TResult, TIntermediate, TResult> simpleCombine, Func<TResult, TResult, TResult> complexCombine)
            {
                this.arg = arg;
                this.action = action;
                this.simpleCombine = simpleCombine;
                this.complexCombine = complexCombine;
            }

            public void Visit<TParent, TChild>(
                Syntax.Project project, TParent parent, IEnumerable<TChild> children,
                Func<TChild, IEnumerable<TNode>> itemFunction)
                where TParent : class
                where TChild : SyntaxNode =>
                Visit(
                    project, parent, children, (arg, action, itemFunction),
                    (t, child) => ForEach(t.itemFunction(child), t.arg, t.action, simpleCombine, complexCombine),
                    complexCombine);

            public void Visit<TParent>(Syntax.Project project, TParent parent, IEnumerable<TNode> items)
                where TParent : class =>
                Visit(project, parent, items, arg, action, simpleCombine);

            private void Visit<TParent, TChild, TVisitArg, TValue>(
                Syntax.Project project, TParent parent, IEnumerable<TChild> children, TVisitArg visitArg,
                Func<TVisitArg, TChild, TValue> childAction, Func<TResult, TValue, TResult> combine)
                where TParent : class
                where TChild : SyntaxNode
            {
                if (project is TransformProject transformProject)
                {
                    Result = transformProject.TransformerBuilder.Collection(
                        parent, children, ActionInvoker.Create(childAction, combine), visitArg);
                }
                else
                {
                    TResult result = default;

                    foreach (var item in children)
                    {
                        result = combine(result, childAction(visitArg, item));
                    }

                    Result = result;
                }
            }
        }

        public static void ForEach<TNode>(IEnumerable<TNode> nodes, Action<TNode> action) where TNode : SyntaxNode
        {
            if (!(nodes is ISyntaxCollection<TNode> syntaxCollection))
                throw new ArgumentException("Collection has to be provided by CSharpE.");
            ClosureChecker.ThrowIfHasClosure(action);

            syntaxCollection.Visit(new Visitor<TNode, Action<TNode>>(action, (a, node) => a(node)));
        }

        public static void ForEach<TNode, T1>(IEnumerable<TNode> nodes, T1 arg1, Action<T1, TNode> action)
            where TNode : SyntaxNode
        {
            if (!(nodes is ISyntaxCollection<TNode> syntaxCollection))
                throw new ArgumentException("Collection has to be provided by CSharpE.");
            ClosureChecker.ThrowIfHasClosure(action);
            GeneralHandler.ThrowIfNotTrackable(arg1);

            syntaxCollection.Visit(new Visitor<TNode, T1>(arg1, action));
        }

        public static IReadOnlyList<TResult> ForEach<TNode, TResult>(
            IEnumerable<TNode> nodes, Func<TNode, TResult> action) where TNode : SyntaxNode =>
            ForEach(nodes, action, (a, node) => a(node));

        public static IReadOnlyList<TResult> ForEach<TNode, TArg, TResult>(
            IEnumerable<TNode> nodes, TArg arg, Func<TArg, TNode, TResult> action) where TNode : SyntaxNode =>
            ForEach<TNode, TArg, TResult, List<TResult>>(nodes, arg, action, AddItemToList, AddListToList) ??
            new List<TResult>();

        private static TResult ForEach<TNode, TArg, TIntermediate, TResult>(
            IEnumerable<TNode> nodes, TArg arg, Func<TArg, TNode, TIntermediate> action,
            Func<TResult, TIntermediate, TResult> simpleCombine, Func<TResult, TResult, TResult> complexCombine)
            where TNode : SyntaxNode
        {
            if (!(nodes is ISyntaxCollection<TNode> syntaxCollection))
                throw new ArgumentException("Collection has to be provided by CSharpE.");
            ClosureChecker.ThrowIfHasClosure(action);
            GeneralHandler.ThrowIfNotTrackable(arg);

            var visitor = new Visitor<TNode, TArg, TIntermediate, TResult>(arg, action, simpleCombine, complexCombine);
            syntaxCollection.Visit(visitor);
            return visitor.Result;
        }

        private static List<TResult> AddItemToList<TResult>(List<TResult> list, TResult item)
        {
            list = list ?? new List<TResult>();
            list.Add(item);
            return list;
        }

        private static List<TResult> AddListToList<TResult>(List<TResult> oldList, List<TResult> newList)
        {
            oldList?.AddRange(newList);
            return oldList ?? newList;
        }

        public static void Segment(TypeDefinition node, Action<ILimitedTypeDefinition> action) =>
            Segment(node, action, (a, n) => a(n));

        public static void Segment<T1>(
            TypeDefinition node, T1 arg1, Action<T1, ILimitedTypeDefinition> action)
        {
            ClosureChecker.ThrowIfHasClosure(action);
            GeneralHandler.ThrowIfNotTrackable(arg1);

            if (node.SourceFile?.Project is TransformProject transformProject)
            {
                transformProject.TransformerBuilder.LimitedSegment(
                    node, ActionInvoker.Create<T1, TypeDefinition>((a1, n) => action(a1, node)), arg1);
            }
            else
            {
                action(arg1, node);
            }
        }

        public static void Segment<T1, T2>(
            TypeDefinition node, T1 arg1, T2 arg2, Action<T1, T2, ILimitedTypeDefinition> action) =>
            Segment(node, (action, arg1, arg2), (t, n) => t.action(t.arg1, t.arg2, n));

    }
}
