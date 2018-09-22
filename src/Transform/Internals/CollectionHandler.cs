using System.Collections;
using System.Linq;

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
                GeneralHandler.ThrowIfNotPersistent(item);
            }
        }

        public static bool Equals<T>(T arg1, T arg2)
        {
            var collection1 = ((IEnumerable)arg1).Cast<object>().ToList();
            var collection2 = ((IEnumerable)arg2).Cast<object>().ToList();

            if (collection1.Count != collection2.Count)
                return false;

            for (int i = 0; i < collection1.Count; i++)
            {
                if (!GeneralHandler.Equals(collection1[i], collection2[i]))
                    return false;
            }

            return true;
        }
    }
}
