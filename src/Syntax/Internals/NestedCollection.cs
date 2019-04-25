using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace CSharpE.Syntax.Internals
{
    internal class NestedCollection<TParent, TChild, TItem> : IEnumerable<TItem>, ISyntaxCollection<TItem>
        where TParent : class
        where TChild : SyntaxNode
    {
        private readonly Project project;
        private readonly TParent parent;
        private readonly IEnumerable<TChild> children;
        private readonly Func<TChild, IEnumerable<TItem>> itemFunction;

        public NestedCollection(
            Project project, TParent parent, IEnumerable<TChild> children,
            Func<TChild, IEnumerable<TItem>> itemFunction)
        {
            this.project = project;
            this.parent = parent;
            this.children = children;
            this.itemFunction = itemFunction;
        }

        public IEnumerator<TItem> GetEnumerator() => children.SelectMany(child => itemFunction(child)).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public void Visit(ISyntaxCollectionVisitor<TItem> visitor) => visitor.Visit(project, parent, children, itemFunction);
    }

    internal static class NestedCollection
    {
        public static IEnumerable<TItem> Create<TChild, TItem>(
            Project parent, IEnumerable<TChild> children, Func<TChild, IEnumerable<TItem>> itemFunction)
            where TChild : SyntaxNode =>
            new NestedCollection<Project, TChild, TItem>(parent, parent, children, itemFunction);

        public static IEnumerable<TItem> Create<TChild, TItem>(
            SyntaxNode parent, IEnumerable<TChild> children, Func<TChild, IEnumerable<TItem>> itemFunction)
            where TChild : SyntaxNode =>
            new NestedCollection<SyntaxNode, TChild, TItem>(parent.SourceFile?.Project, parent, children, itemFunction);
    }
}