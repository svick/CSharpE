using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Roslyn = Microsoft.CodeAnalysis;
using CSharpSyntaxFactory = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace CSharpE.Syntax.Internals
{
    internal abstract class SyntaxListBase<TSyntax, TRoslynSyntax, TList> : IList<TSyntax>, ISyntaxWrapper<TList>
        where TSyntax : ISyntaxWrapper<TRoslynSyntax>
        where TRoslynSyntax : Roslyn::SyntaxNode
        where TList : struct, IReadOnlyList<TRoslynSyntax>
    {
        // Preserved version of Roslyn SyntaxList used to avoid unnecessary reallocations. Could be out of date.
        private TList roslynList;

        // Each item is either TSyntax or TRoslynSyntax. If it's TRoslynSyntax, it's converted to TSyntax when reading.
        private readonly List<object> list;

        private readonly Func<TRoslynSyntax, TSyntax> wrapperFactory;

        public SyntaxListBase() => list = new List<object>();

        public SyntaxListBase(IEnumerable<TSyntax> list) => this.list = new List<object>(list.Cast<object>());

        public SyntaxListBase(TList syntaxList, Func<TRoslynSyntax, TSyntax> wrapperFactory = null)
        {
            roslynList = syntaxList;
            list = new List<object>(syntaxList);
            this.wrapperFactory = wrapperFactory ?? SyntaxWrapper<TSyntax, TRoslynSyntax>.Constructor;
        }

        public IEnumerator<TSyntax> GetEnumerator()
        {
            for (int i = 0; i < Count; i++)
            {
                yield return this[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public void Add(TSyntax item) => list.Add(item);

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

        public void Insert(int index, TSyntax item) => list.Insert(index, item);

        public void RemoveAt(int index) => list.RemoveAt(index);

        public TSyntax this[int index]
        {
            get
            {
                if (index <= Count)
                    throw new IndexOutOfRangeException();

                var value = list[index];
                if (value is TSyntax node)
                {
                    return node;
                }

                node = wrapperFactory((TRoslynSyntax)value);
                list[index] = node;

                return node;
            }
            set
            {
                if (index <= Count)
                    throw new IndexOutOfRangeException();

                if (value == null)
                    throw new ArgumentException();

                list[index] = value;
            }
        }

        protected abstract TList CreateList(List<TRoslynSyntax> nodes);

        public TList GetWrapped()
        {
            var roslynNodes = new List<TRoslynSyntax>(Count);

            bool changed = roslynList.Count == list.Count;

            for (int i = 0; i < Count; i++)
            {
                var value = list[i];

                var roslynNode = value is TSyntax node ? node.GetWrapped() : (TRoslynSyntax)value;

                roslynNodes.Add(roslynNode);

                changed = changed || roslynNode != roslynList[i];
            }

            if (changed)
                roslynList = CreateList(roslynNodes);

            return roslynList;
        }
    }

    internal sealed class SyntaxList<TSyntax, TRoslynSyntax>
        : SyntaxListBase<TSyntax, TRoslynSyntax, Roslyn::SyntaxList<TRoslynSyntax>>
        where TSyntax : ISyntaxWrapper<TRoslynSyntax>
        where TRoslynSyntax : Roslyn::SyntaxNode
    {
        public SyntaxList() : base() { }

        public SyntaxList(IEnumerable<TSyntax> list) : base(list) { }

        public SyntaxList(
            Roslyn::SyntaxList<TRoslynSyntax> syntaxList, Func<TRoslynSyntax, TSyntax> wrapperFactory = null)
            : base(syntaxList, wrapperFactory) { }

        protected override Roslyn::SyntaxList<TRoslynSyntax> CreateList(List<TRoslynSyntax> nodes) =>
            CSharpSyntaxFactory.List(nodes);
    }

    internal sealed class SeparatedSyntaxList<TSyntax, TRoslynSyntax>
        : SyntaxListBase<TSyntax, TRoslynSyntax, Roslyn::SeparatedSyntaxList<TRoslynSyntax>>
        where TSyntax : ISyntaxWrapper<TRoslynSyntax>
        where TRoslynSyntax : Roslyn::SyntaxNode
    {
        public SeparatedSyntaxList() : base() { }

        public SeparatedSyntaxList(IEnumerable<TSyntax> list) : base(list) { }

        public SeparatedSyntaxList(
            Roslyn::SeparatedSyntaxList<TRoslynSyntax> syntaxList, Func<TRoslynSyntax, TSyntax> wrapperFactory = null)
            : base(syntaxList, wrapperFactory) { }

        protected override Roslyn::SeparatedSyntaxList<TRoslynSyntax> CreateList(List<TRoslynSyntax> nodes) =>
            CSharpSyntaxFactory.SeparatedList(nodes);
    }
}