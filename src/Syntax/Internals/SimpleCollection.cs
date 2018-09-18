using System.Collections;
using System.Collections.Generic;

namespace CSharpE.Syntax.Internals
{
    public class SimpleCollection<TParent, TItem> : IEnumerable<TItem>, ISyntaxCollection<TItem>
        where TParent : class
    {
        private readonly Project project;
        private readonly TParent parent;
        private readonly IEnumerable<TItem> items;

        public SimpleCollection(Project project, TParent parent, IEnumerable<TItem> items)
        {
            this.project = project;
            this.parent = parent;
            this.items = items;
        }

        public IEnumerator<TItem> GetEnumerator() => items.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public void Visit(ISyntaxCollectionVisitor<TItem> visitor) => visitor.Visit(project, parent, items);
    }

    internal static class SimpleCollection
    {
        public static IEnumerable<TItem> Create<TItem>(Project parent, IEnumerable<TItem> items) =>
            new SimpleCollection<Project, TItem>(parent, parent, items);

        public static IEnumerable<TItem> Create<TParent, TItem>(TParent parent, IEnumerable<TItem> items)
            where TParent : SyntaxNode =>
            new SimpleCollection<TParent, TItem>(parent.SourceFile?.Project, parent, items);
    }
}