using System.Collections.Generic;

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

            return type.IsGenericType && type.GetGenericTypeDefinition().FullName.StartsWith("System.ValueTuple`");
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
                Persistence.ThrowIfNotPersistent(item);
            }
        }
    }
}