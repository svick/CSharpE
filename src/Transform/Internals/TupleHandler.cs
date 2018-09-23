using System;
using System.Collections.Generic;
using System.Linq;

namespace CSharpE.Transform.Internals
{
    // Note: in .Net Standard 2.1, this could use ITuple 
    internal static class TupleHandler
    {
        public static bool IsTuple<T>(T arg)
        {
            if (arg == null)
                return false;

            var type = arg.GetType();

            return type.IsGenericType &&
                   type.GetGenericTypeDefinition().FullName?.StartsWith("System.ValueTuple`") == true;
        }

        private static IEnumerable<object> GetItems<T>(T arg)
        {
            // PERF
            
            var fields = arg.GetType().GetFields();

            foreach (var field in fields)
            {
                yield return field.GetValue(arg);
            }
        }

        public static void ThrowIfNotPersistent<T>(T arg)
        {
            foreach (var item in GetItems(arg))
            {
                GeneralHandler.ThrowIfNotPersistent(item);
            }
        }

        public static bool IsImmutable<T>(T arg) => GetItems(arg).All(GeneralHandler.IsImmutable);

        public static T DeepClone<T>(T input)
        {
            var clones = GetItems(input).Select(GeneralHandler.DeepClone);

            return (T)Activator.CreateInstance(input.GetType(), clones.ToArray());
        }

        public static bool Equals<T>(T arg1, T arg2) => CollectionHandler.Equals(GetItems(arg1), GetItems(arg2));
    }
}
