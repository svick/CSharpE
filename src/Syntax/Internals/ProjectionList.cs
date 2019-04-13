using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace CSharpE.Syntax.Internals
{
    internal static class ProjectionList
    {
        public static IList<TTarget> Create<TSource, TTarget>(
            IList<TSource> sourceList, Func<TSource, TTarget> projection, Func<TTarget, TSource> reverseProjection)
        {
            return new ProjectionList<TSource, TTarget>(sourceList, projection, reverseProjection);
        }
    }

    // PERF: this type is currently mostly not optimized.
    internal class ProjectionList<TSource, TTarget> : IList<TTarget>
    {
        private readonly IList<TSource> sourceList;
        private readonly Func<TSource, TTarget> projection;
        private readonly Func<TTarget, TSource> reverseProjection;

        public ProjectionList(
            IList<TSource> sourceList, Func<TSource, TTarget> projection, Func<TTarget, TSource> reverseProjection)
        {
            this.sourceList = sourceList;
            this.projection = projection;
            this.reverseProjection = reverseProjection;
        }

        private IEnumerable<TTarget> GetEnumerable() => sourceList.Select(projection);

        public IEnumerator<TTarget> GetEnumerator() => GetEnumerable().GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public void Add(TTarget item) => sourceList.Add(reverseProjection(item));

        public void Clear() => sourceList.Clear();

        public bool Contains(TTarget item) => GetEnumerable().Contains(item);

        public void CopyTo(TTarget[] array, int arrayIndex) => GetEnumerable().ToList().CopyTo(array, arrayIndex);

        public bool Remove(TTarget item) => sourceList.Remove(reverseProjection(item));

        public int Count => sourceList.Count;
        
        public bool IsReadOnly => false;
        
        public int IndexOf(TTarget item) => GetEnumerable().ToList().IndexOf(item);

        public void Insert(int index, TTarget item) => sourceList.Insert(index, reverseProjection(item));

        public void RemoveAt(int index) => sourceList.RemoveAt(index);

        public TTarget this[int index]
        {
            get => projection(sourceList[index]);
            set => sourceList[index] = reverseProjection(value);
        }
    }
}