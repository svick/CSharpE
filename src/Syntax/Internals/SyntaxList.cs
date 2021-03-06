﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Roslyn = Microsoft.CodeAnalysis;
using RoslynSyntaxFactory = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace CSharpE.Syntax.Internals
{
    internal abstract class SyntaxListBase
    {
        internal abstract SyntaxNode Parent { get; set; }
    }

    internal abstract class SyntaxListBase<TSyntax, TRoslynSyntax, TList> : SyntaxListBase, IList<TSyntax>, ISyntaxWrapper<TList>, ISyntaxCollection<TSyntax>
        where TSyntax : ISyntaxWrapper<TRoslynSyntax>
        where TRoslynSyntax : Roslyn::SyntaxNode
        where TList : IReadOnlyList<TRoslynSyntax>
    {
        // Preserved version of Roslyn SyntaxList used to avoid unnecessary reallocations. Could be out of date.
        private TList roslynList;

        // Each item is either TSyntax or TRoslynSyntax. If it's TRoslynSyntax, it's converted to TSyntax when reading.
        private readonly List<object> list;

        private SyntaxNode parent;
        internal sealed override SyntaxNode Parent
        {
            get => parent;
            set
            {
                parent = value;

                // PERF: check is TSyntax is assignable to SyntaxNode
                foreach (var o in list)
                {
                    if (o is SyntaxNode node)
                        node.Parent = value;
                }
            }
        }

        protected SyntaxListBase(SyntaxNode parent)
        {
            list = new List<object>();

            Parent = parent;
        }

        protected SyntaxListBase(IEnumerable<TSyntax> list, SyntaxNode parent)
        {
            this.parent = parent;

            this.list = list?.Select(syntax => (object)WithParent(syntax)).ToList() ?? new List<object>();
        }

        protected SyntaxListBase(TList syntaxList, SyntaxNode parent)
        {
            roslynList = syntaxList;

            // "compacted" nodes have to be expanded right now, so that list size is correct

            list = new List<object>(syntaxList.Count);

            foreach (var roslynSyntax in syntaxList)
            {
                if (FromRoslyn.IsCompacted(roslynSyntax))
                    list.AddRange(FromRoslyn.Expand(roslynSyntax));
                else
                    list.Add(roslynSyntax);
            }

            Parent = parent;
        }

        private TSyntax WithParent(TSyntax syntax) =>
            syntax is SyntaxNode node ? (TSyntax)(object)SyntaxNode.WithParent(node, Parent) : syntax;

        public IEnumerator<TSyntax> GetEnumerator()
        {
            for (int i = 0; i < Count; i++)
            {
                yield return this[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public void Add(TSyntax item) => list.Add(WithParent(item));

        public void Clear() => list.Clear();

        public bool Contains(TSyntax item) => list.Contains(item);

        public void CopyTo(TSyntax[] array, int arrayIndex)
        {
            // TODO: input validation

            for (int i = 0; i < Count; i++)
            {
                array[arrayIndex + i] = this[i];
            }
        }

        public bool Remove(TSyntax item) => list.Remove(item);

        public int Count => list.Count;

        public bool IsReadOnly => false;

        public int IndexOf(TSyntax item) => list.IndexOf(item);

        public void Insert(int index, TSyntax item) => list.Insert(index, WithParent(item));

        public void RemoveAt(int index) => list.RemoveAt(index);

        public TSyntax this[int index]
        {
            get
            {
                if (index >= Count)
                    throw new IndexOutOfRangeException();

                var value = list[index];
                if (value is TSyntax node)
                    return node;

                node = CreateWrapper((TRoslynSyntax)value);
                list[index] = node;

                return node;
            }
            set
            {
                if (index <= Count)
                    throw new IndexOutOfRangeException();

                if (value == null)
                    throw new ArgumentException();

                list[index] = WithParent(value);
            }
        }

        protected abstract TList CreateList(IEnumerable<TRoslynSyntax> nodes);

        protected virtual TSyntax CreateWrapper(TRoslynSyntax roslynSyntax)
        {
            var wrapper = SyntaxWrapper<TSyntax, TRoslynSyntax>.Constructor(roslynSyntax);

            if (wrapper is SyntaxNode node)
                node.Parent = parent;

            return wrapper;
        }

        private ChangeTracker changeTracker = new ChangeTracker();

        TList ISyntaxWrapper<TList>.GetWrapped(ref bool? changed)
        {
            changeTracker.GetAndResetChanged(ref changed);

            // PERF: don't allocate roslynNodes before it's known something changed

            var roslynNodes = new List<TRoslynSyntax>(Count);

            bool? thisChanged = roslynList?.Count != list.Count;

            for (int i = 0; i < Count; i++)
            {
                var value = list[i];

                var roslynNode = value is TSyntax node ? node.GetWrapped(ref thisChanged) : (TRoslynSyntax)value;

                roslynNodes.Add(roslynNode);
            }

            if (thisChanged == true)
            {
                roslynList = CreateList(roslynNodes);

                changeTracker.SetChanged(ref changed);
            }

            return roslynList;
        }

        void ISyntaxCollection<TSyntax>.Visit(ISyntaxCollectionVisitor<TSyntax> visitor) => visitor.Visit(Parent, this);
    }

    internal class SyntaxList<TSyntax, TRoslynSyntax>
        : SyntaxListBase<TSyntax, TRoslynSyntax, Roslyn::SyntaxList<TRoslynSyntax>>
        where TSyntax : ISyntaxWrapper<TRoslynSyntax>
        where TRoslynSyntax : Roslyn::SyntaxNode
    {
        internal SyntaxList(SyntaxNode parent) : base(parent) { }

        internal SyntaxList(IEnumerable<TSyntax> list, SyntaxNode parent) : base(list, parent) { }

        internal SyntaxList(
            Roslyn::SyntaxList<TRoslynSyntax> syntaxList, SyntaxNode parent) : base(syntaxList, parent) { }

        protected sealed override Roslyn::SyntaxList<TRoslynSyntax> CreateList(IEnumerable<TRoslynSyntax> nodes) =>
            RoslynSyntaxFactory.List(nodes);
    }

    internal class SeparatedSyntaxList<TSyntax, TRoslynSyntax>
        : SyntaxListBase<TSyntax, TRoslynSyntax, Roslyn::SeparatedSyntaxList<TRoslynSyntax>>
        where TSyntax : ISyntaxWrapper<TRoslynSyntax>
        where TRoslynSyntax : Roslyn::SyntaxNode
    {
        internal SeparatedSyntaxList(SyntaxNode parent) : base(parent) { }

        internal SeparatedSyntaxList(IEnumerable<TSyntax> list, SyntaxNode parent) : base(list, parent) { }

        internal SeparatedSyntaxList(
            Roslyn::SeparatedSyntaxList<TRoslynSyntax> syntaxList, SyntaxNode parent) : base(syntaxList, parent) { }

        protected override Roslyn::SeparatedSyntaxList<TRoslynSyntax> CreateList(IEnumerable<TRoslynSyntax> nodes) =>
            RoslynSyntaxFactory.SeparatedList(nodes);
    }
}