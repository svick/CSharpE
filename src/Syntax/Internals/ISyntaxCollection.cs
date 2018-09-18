using System;
using System.Collections.Generic;

namespace CSharpE.Syntax.Internals
{
    public interface ISyntaxCollection<T>
    {
        void Visit(ISyntaxCollectionVisitor<T> visitor);
    }

    public interface ISyntaxCollectionVisitor<T>
    {
        void Visit<TParent, TChild>(
            Project project, TParent parent, IEnumerable<TChild> children, Func<TChild, IEnumerable<T>> itemFunction)
            where TParent : class
            where TChild : SyntaxNode;

        void Visit<TParent>(Project project, TParent parent, IEnumerable<T> items) where TParent : class;
    }

    public static class SyntaxCollectionVisitorExtensions
    {
        public static void Visit<T, TParent>(
            this ISyntaxCollectionVisitor<T> visitor, TParent parent, IEnumerable<T> items)
            where TParent : SyntaxNode =>
            visitor.Visit(parent?.SourceFile?.Project, parent, items);
    }
}