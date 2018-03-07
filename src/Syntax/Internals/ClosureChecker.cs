using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace CSharpE.Syntax.Internals
{
    internal static class ClosureChecker
    {
        public static void ThrowIfHasClosure(Delegate d)
        {
            if (HasClosure(d))
                throw new HasClosureException($"The delegate for {EnhancedStackTrace.GetMethodDisplayString(d.GetMethodInfo())} has closure."); // TODO: custom exception type
        }

        private static readonly ConcurrentDictionary<Type, bool> HasClosureCache = new ConcurrentDictionary<Type, bool>();

        public static bool HasClosure(Delegate d)
        {
            if (d.Target == null)
                return false;

            return HasClosureCache.GetOrAdd(d.Target.GetType(), IsClosure);
        }

        private static bool IsClosure(Type type) => type.GetRuntimeFields().Any(f => !f.IsStatic);
    }

    internal class HasClosureException : Exception
    {
        public HasClosureException(string message) : base(message) { }
    }
}