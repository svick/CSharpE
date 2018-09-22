using System;
using System.Collections;
using System.Collections.Generic;
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

        private static T[] DeepCloneArray<T>(T[] array) => Array.ConvertAll(array, GeneralHandler.DeepClone);

        private static List<T> DeepCloneList<T>(List<T> list) => list.ConvertAll(GeneralHandler.DeepClone);

        public static T DeepClone<T>(T input)
        {
            var type = input.GetType();

            // the second condition makes sure the array is not "weird" (i.e. a multi-dimensional array, or array with lower bound other than 0)
            if (type.IsArray && type == type.GetElementType().MakeArrayType())
                return DeepCloneArray((dynamic)input);

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
                return DeepCloneList((dynamic)input);

            throw new InvalidOperationException($"The collection type {input.GetType()} cannot be cloned.");
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
