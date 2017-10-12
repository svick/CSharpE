using System;
using System.Collections;
using System.Collections.Generic;
using Roslyn = Microsoft.CodeAnalysis;
using CSharpSyntaxFactory = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace CSharpE.Syntax.Internals
{
    internal class SyntaxList<TSyntax, TRoslynSyntax> : IList<TSyntax>, ISyntaxWrapper<Roslyn::SyntaxList<TRoslynSyntax>>
        where TSyntax : class, ISyntaxWrapper<TRoslynSyntax>
        where TRoslynSyntax : Roslyn::SyntaxNode
    {
        // Preserved version of Roslyn SyntaxList used to avoid unnecessary reallocations. Could be out of date.
        private Roslyn::SyntaxList<TRoslynSyntax> roslynList;

        // Each item is either TSyntax or TRoslynSyntax. If it's TRoslynSyntax, it's converted to TSyntax when reading.
        private readonly List<object> list;

        public SyntaxList() => list = new List<object>();

        public SyntaxList(Roslyn::SyntaxList<TRoslynSyntax> syntaxList)
        {
            roslynList = syntaxList;
            list = new List<object>(syntaxList);
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

                node = SyntaxWrapper<TSyntax, TRoslynSyntax>.Create((TRoslynSyntax)value);
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

        public Roslyn::SyntaxList<TRoslynSyntax> GetWrapped()
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
                roslynList = CSharpSyntaxFactory.List(roslynNodes);

            return roslynList;
        }
    }
}