using System;
using CSharpE.Syntax.Internals;

namespace CSharpE.Transform.Internals
{
    internal static class ArgumentChecker
    {
        // TODO: rename "persistent" to something better
        public static void ThrowIfNotPersistent<T>(T arg)
        {
            if (arg == null)
                return;

            if (arg is IPersistent)
                return;

            // delegates without closures are considered persistent
            if (arg is Delegate del && !ClosureChecker.HasClosure(del))
                return;

            throw new NotPersisitentException($"The given {arg.GetType()} is not persistent.");
        }
    }

    internal class NotPersisitentException : Exception
    {
        public NotPersisitentException(string message) : base(message) { }
    }

}