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

    }
}