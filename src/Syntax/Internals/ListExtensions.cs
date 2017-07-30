using System;
using System.Collections.Generic;

namespace CSharpE.Syntax.Internals
{
    static class ListExtensions
    {
        public static void AddRange<T>(this IList<T> list, IEnumerable<T> collection)
        {
            foreach (var value in collection)
            {
                list.Add(value);
            }
        }

        public static void RemoveAll<T>(this IList<T> list, Predicate<T> match)
        {
            // Note: a more efficient and complicated version would be similar to List<T>.RemoveAll():
            // move non-matching items over matching ones and then RemoveAt remnants at the end

            for (int i = list.Count - 1; i >= 0; i--)
            {
                if (match(list[i]))
                    list.RemoveAt(i);
            }
        }
    }
}