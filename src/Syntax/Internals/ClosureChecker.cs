using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using static System.Reflection.BindingFlags;

namespace CSharpE.Syntax.Internals
{
    internal static class ClosureChecker
    {
        public static void ThrowIfHasClosure(Delegate d)
        {
            var closureInfo = GetClosureInfo(d);

            if (closureInfo != null)
            {
                string message = $"The delegate for {EnhancedStackTrace.GetMethodDisplayString(d.Method)} has closure.";

                if (closureInfo != string.Empty)
                    message = $"{message} Closure contains {closureInfo}.";

                throw new HasClosureException(message);
            }
        }

        private static readonly ConcurrentDictionary<Type, string> HasClosureCache = new ConcurrentDictionary<Type, string>();

        private static string GetClosureInfo(Delegate d)
        {
            if (d.Target == null)
                return null;

            return HasClosureCache.GetOrAdd(d.Target.GetType(), CreateClosureInfo);
        }

        private static string CreateClosureInfo(Type type)
        {
            var fields = type.GetFields(Public | NonPublic | Instance);

            if (!fields.Any())
                return null;

            if (type.GetCustomAttribute<CompilerGeneratedAttribute>() == null)
                return type.FullName;

            var field = fields.First();

            return $"{field.FieldType.FullName} {field.Name}";
        }
    }

    internal class HasClosureException : Exception
    {
        public HasClosureException(string message) : base(message) { }
    }
}