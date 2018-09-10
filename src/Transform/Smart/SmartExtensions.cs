using System;
using System.Collections.Generic;
using CSharpE.Syntax;
using CSharpE.Transform.Internals;

namespace CSharpE.Transform.Smart
{
    public static class SmartExtensions
    {
        private static void ForEach<TParent, TItem>(
            TParent parent, Syntax.Project project, Action<TItem> action,
            Func<TParent, IEnumerable<TItem>> collectionFunction)
            where TParent : class
            where TItem : SyntaxNode
        {
            ClosureChecker.ThrowIfHasClosure(action);

            if (project is TransformProject transformProject)
            {
                transformProject.TransformerBuilder.Collection(
                    parent, collectionFunction, ActionInvoker.Create(action), Unit.Value);
            }
            else
            {
                foreach (var item in collectionFunction(parent))
                {
                    action(item);
                }
            }
        }

        private static void ForEach<TItem>(
            Syntax.Project project, Action<TItem> action,
            Func<Syntax.Project, IEnumerable<TItem>> collectionFunction) where TItem : SyntaxNode
            => ForEach(project, project, action, collectionFunction);

        private static void ForEach<TParent, TItem>(
            TParent parent, Action<TItem> action, Func<TParent, IEnumerable<TItem>> collectionFunction)
            where TParent : SyntaxNode
            where TItem : SyntaxNode
            => ForEach(parent, parent.SourceFile?.Project, action, collectionFunction);

        private static void ForEach<TParent, TItem, T1>(
            TParent parent, Syntax.Project project, T1 arg1, Action<T1, TItem> action,
            Func<TParent, IEnumerable<TItem>> collectionFunction)
            where TParent : class
            where TItem : SyntaxNode
        {
            ClosureChecker.ThrowIfHasClosure(action);
            Persistence.ThrowIfNotPersistent(arg1);

            if (project is TransformProject transformProject)
            {
                transformProject.TransformerBuilder.Collection(
                    parent, collectionFunction, ActionInvoker.Create(action), arg1);
            }
            else
            {
                foreach (var item in collectionFunction(parent))
                {
                    action(arg1, item);
                }
            }
        }

        private static IReadOnlyList<TResult> ForEach<TParent, TItem, TResult>(
            TParent parent, Func<TItem, TResult> action,
            Func<TParent, IEnumerable<TItem>> collectionFunction)
            where TParent : SyntaxNode
            where TItem : SyntaxNode
            => ForEach(parent, parent.SourceFile?.Project, action, collectionFunction);

        private static IReadOnlyList<TResult> ForEach<TParent, TItem, TResult>(
            TParent parent, Syntax.Project project, Func<TItem, TResult> action,
            Func<TParent, IEnumerable<TItem>> collectionFunction)
            where TParent : class
            where TItem : SyntaxNode
        {
            ClosureChecker.ThrowIfHasClosure(action);

            List<TResult> result;

            if (project is TransformProject transformProject)
            {
                result = transformProject.TransformerBuilder.Collection(
                    parent, collectionFunction, ActionInvoker.Create<TItem, TResult, List<TResult>>(
                        action, (maybeList, item) =>
                        {
                            var list = maybeList ?? new List<TResult>();
                            list.Add(item);
                            return list;
                        }), Unit.Value);
            }
            else
            {
                result = new List<TResult>();
                
                foreach (var item in collectionFunction(parent))
                {
                    result.Add(action(item));
                }
            }

            return result.AsReadOnly();
        }

        private static void ForEach<TItem, T1>(
            Syntax.Project project, T1 arg1, Action<T1, TItem> action,
            Func<Syntax.Project, IEnumerable<TItem>> collectionFunction) where TItem : SyntaxNode
            => ForEach(project, project, arg1, action, collectionFunction);

        private static void ForEach<TParent, TItem, T1>(
            TParent parent, T1 arg1, Action<T1, TItem> action, Func<TParent, IEnumerable<TItem>> collectionFunction)
            where TParent : SyntaxNode
            where TItem : SyntaxNode
            => ForEach(parent, parent.SourceFile?.Project, arg1, action, collectionFunction);

        // TODO: limit type kind and use in ActorTransformation
        public static void ForEachTypeWithAttribute<TAttribute>(
            this Syntax.Project project, Action<BaseTypeDefinition> action)
            where TAttribute : System.Attribute =>
            project.ForEachSourceFile(action, (a, sourceFile) => sourceFile.ForEachTypeWithAttribute<TAttribute>(a));

        public static void ForEachTypeWithAttribute<TAttribute>(
            this Syntax.SourceFile sourceFile, Action<BaseTypeDefinition> action)
            where TAttribute : System.Attribute =>
            ForEach(sourceFile, action, f => f.GetTypesWithAttribute<TAttribute>());

        public static void ForEachTypeWithAttribute<TAttribute, T1>(
            this Syntax.Project project, T1 arg1, Action<T1, BaseTypeDefinition> action)
            where TAttribute : System.Attribute =>
            project.ForEachSourceFile((arg1, action),
                (t, sourceFile) => sourceFile.ForEachTypeWithAttribute<TAttribute, T1>(t.arg1, t.action));

        public static void ForEachTypeWithAttribute<TAttribute, T1>(
            this Syntax.SourceFile sourceFile, T1 arg1, Action<T1, BaseTypeDefinition> action)
            where TAttribute : System.Attribute =>
            ForEach(sourceFile, arg1, action, f => f.GetTypesWithAttribute<TAttribute>());

        public static void ForEachSourceFile(
            this Syntax.Project project, Action<Syntax.SourceFile> action) =>
            ForEach(project, action, p => p.SourceFiles);

        public static void ForEachSourceFile<T1>(
            this Syntax.Project project, T1 arg1, Action<T1, Syntax.SourceFile> action) =>
            ForEach(project, arg1, action, p => p.SourceFiles);

        public static void ForEachPublicMethod(this TypeDefinition type, Action<MethodDefinition> action) =>
            ForEach(type, action, t => t.PublicMethods);

        public static void ForEachPublicMethod<T1>(
            this TypeDefinition type, T1 arg1, Action<T1, MethodDefinition> action) =>
            ForEach(type, arg1, action, t => t.PublicMethods);

        public static void ForEachField(this TypeDefinition type, Action<FieldDefinition> action) =>
            ForEach(type, action, t => t.Fields);

        public static IReadOnlyList<TResult> ForEachField<TResult>(
            this TypeDefinition type, Func<FieldDefinition, TResult> action) =>
            ForEach(type, action, t => t.Fields);
        
        public static void LimitedSegment(this TypeDefinition node, Action<ILimitedTypeDefinition> action)
            => LimitedSegment(node, node.SourceFile?.Project, action);

        private static void LimitedSegment(
            TypeDefinition node, Syntax.Project project, Action<ILimitedTypeDefinition> action)
        {
            ClosureChecker.ThrowIfHasClosure(action);
            
            if (project is TransformProject transformProject)
            {
                transformProject.TransformerBuilder.LimitedSegment(
                    node, ActionInvoker.Create<TypeDefinition>(action), Unit.Value);
            }
            else
            {
                action(node);
            }
        }

        public static void LimitedSegment<T1>(
            this TypeDefinition node, T1 arg1, Action<T1, ILimitedTypeDefinition> action)
            => LimitedSegment(node, node.SourceFile?.Project, arg1, action);

        private static void LimitedSegment<T1>(TypeDefinition node, Syntax.Project project, T1 arg1,
            Action<T1, ILimitedTypeDefinition> action)
        {
            ClosureChecker.ThrowIfHasClosure(action);
            Persistence.ThrowIfNotPersistent(arg1);
            
            if (project is TransformProject transformProject)
            {
                transformProject.TransformerBuilder.LimitedSegment(
                    node, ActionInvoker.Create<T1, TypeDefinition>((a1, n) => action(a1, node)), arg1);
            }
            else
            {
                action(arg1, node);
            }
        }

        public static void LimitedSegment<T1, T2>(
            this TypeDefinition node, T1 arg1, T2 arg2, Action<T1, T2, ILimitedTypeDefinition> action)
            => LimitedSegment(node, node.SourceFile?.Project, arg1, arg2, action);

        private static void LimitedSegment<T1, T2>(TypeDefinition node, Syntax.Project project, T1 arg1, T2 arg2,
            Action<T1, T2, ILimitedTypeDefinition> action)
        {
            ClosureChecker.ThrowIfHasClosure(action);
            Persistence.ThrowIfNotPersistent(arg1);
            
            if (project is TransformProject transformProject)
            {
                transformProject.TransformerBuilder.LimitedSegment(node,
                    ActionInvoker.Create<(T1, T2), TypeDefinition>((tuple, n) => action(tuple.Item1, tuple.Item2, n)),
                    (arg1, arg2));
            }
            else
            {
                action(arg1, arg2, node);
            }
        }

    }
}