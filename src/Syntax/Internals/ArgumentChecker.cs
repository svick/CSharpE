using System;

namespace CSharpE.Syntax.Internals
{
    internal static class ArgumentChecker
    {
        public static void ThrowIfNotPersistent<T>(T arg)
        {
            if (arg == null)
                return;

            if (arg is IPersistent)
                return;

            throw new NotPersisitentException($"The type {arg.GetType()} is not persistent.");
        }
    }

    internal class NotPersisitentException : Exception
    {
        public NotPersisitentException(string message) : base(message) { }
    }

}