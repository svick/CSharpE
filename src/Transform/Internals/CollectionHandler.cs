using System.Collections;

namespace CSharpE.Transform.Internals
{
    internal static class CollectionHandler
    {
        public static bool IsCollection<T>(T arg) => arg is IEnumerable && !(arg is string);

        public static void ThrowIfNotPersistent<T>(T arg)
        {
            var collection = (IEnumerable)arg;

            foreach (var item in collection)
            {
                Persistence.ThrowIfNotPersistent(item);
            }
        }
    }
}