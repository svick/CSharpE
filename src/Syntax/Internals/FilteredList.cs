using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace CSharpE.Syntax.Internals
{
    internal static class FilteredList
    {
        public static IList<TTarget> Create<TSource, TTarget>(IList<TSource> sourceList)
            where TTarget : TSource
        {
            return new FilteredList<TSource, TTarget>(sourceList);
        }

        public static IList<TTarget> Create<TSource, TTarget>(IList<TSource> sourceList, Func<TTarget, bool> filter)
            where TTarget : TSource
        {
            return new FilteredList<TSource, TTarget>(sourceList, filter);
        }

        public static IList<T> Create<T>(IList<T> sourceList, Func<T, bool> filter)
        {
            return new FilteredList<T, T>(sourceList, filter);
        }

        public static void Set<TSource, TTarget>(
            IList<TSource> sourceList, Func<TTarget, bool> filter, IList<TTarget> values)
            where TTarget : TSource
        {
            var filteredList = Create(sourceList, filter);
            filteredList.Clear();
            sourceList.AddRange((IEnumerable<TSource>)values);
        }

        public static void Set<TSource, TTarget>(IList<TSource> sourceList, IList<TTarget> values)
            where TTarget : TSource
        {
            Set(sourceList, null, values);
        }
    }

    // Note: this type is currently mostly not optimized for perf.
    internal class FilteredList<TSource, TTarget> : IList<TTarget> where TTarget : TSource
    {
        private readonly IList<TSource> sourceList;
        private readonly Func<TTarget, bool> filter;

        public FilteredList(IList<TSource> sourceList, Func<TTarget, bool> filter = null)
        {
            this.sourceList = sourceList;
            this.filter = filter;
        }

        private IEnumerable<TTarget> GetEnumerable()
        {
            IEnumerable<TTarget> enumerable;

            if (typeof(TSource) == typeof(TTarget))
                enumerable = (IEnumerable<TTarget>)(object)sourceList;
            else
                enumerable = sourceList.OfType<TTarget>();

            if (filter != null)
                enumerable = enumerable.Where(filter);

            return enumerable;
        }

        public IEnumerator<TTarget> GetEnumerator() => GetEnumerable().GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public void Add(TTarget item) => sourceList.Add(item);

        public void Clear()
        {
            // TODO: tests
            Predicate<TSource> predicate;

            if (typeof(TSource) == typeof(TTarget))
            {
                if (filter == null)
                    predicate = _ => true;
                else
                    predicate = ((Func<TSource, bool>)(object)filter).Invoke;
            }
            else
            {
                if (filter == null)
                    predicate = item => item is TTarget;
                else
                    predicate = item => item is TTarget castedItem && filter(castedItem);
            }
            
            sourceList.RemoveAll(predicate);
        }

        public bool Contains(TTarget item) => GetEnumerable().Contains(item);

        public void CopyTo(TTarget[] array, int arrayIndex) => GetEnumerable().ToList().CopyTo(array, arrayIndex);

        public bool Remove(TTarget item)
        {
            if (GetEnumerable().Contains(item))
                return sourceList.Remove(item);

            return false;
        }

        public int Count => GetEnumerable().Count();
        
        public bool IsReadOnly => false;
        
        public int IndexOf(TTarget item) => GetEnumerable().ToList().IndexOf(item);

        public void Insert(int index, TTarget item)
        {
            throw new NotImplementedException();
        }

        public void RemoveAt(int index)
        {
            // TODO: this assumes sourceList does not contain duplicates
            
            var item = this[index];
            sourceList.Remove(item);
        }

        public TTarget this[int index]
        {
            get => GetEnumerable().ElementAt(index);
            set
            {
                // TODO: this assumes sourceList does not contain duplicates
                int sourceIndex = sourceList.IndexOf(GetEnumerable().ElementAt(index));

                sourceList[sourceIndex] = value;
            }
        }
    }
}