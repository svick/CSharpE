using System;
using System.Collections.Generic;
using System.Linq;

namespace CSharpE.Syntax.Internals
{
    public static class TupleExtensions
    {
        public static void Deconstruct<TKey, TValue>(
            this KeyValuePair<TKey, TValue> keyValuePair, out TKey key, out TValue value)
        {
            key = keyValuePair.Key;
            value = keyValuePair.Value;
        }

        public static IEnumerable<KeyValuePair<TKey, TValue>> Where<TKey, TValue>(
            this IEnumerable<KeyValuePair<TKey, TValue>> source, Func<TKey, TValue, bool> predicate)
            => source.Where(kvp => predicate(kvp.Key, kvp.Value));

        public static IEnumerable<TResult> Select<TKey, TValue, TResult>(
            this IEnumerable<KeyValuePair<TKey, TValue>> source, Func<TKey, TValue, TResult> selector)
            => source.Select(kvp => selector(kvp.Key, kvp.Value));

        public static IEnumerable<(T1, T2)> Zip<T1, T2>(this IEnumerable<T1> first, IEnumerable<T2> second) =>
            first.Zip(second, (x, y) => (x, y));
    }
}