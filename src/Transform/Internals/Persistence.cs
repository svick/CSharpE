using System;
using CSharpE.Syntax;

namespace CSharpE.Transform.Internals
{
    internal static class Persistence
    {
        public static void ThrowIfNotPersistent<T>(T arg)
        {
            if (arg == null)
                return;

            // disconnected syntax nodes are fine
            if (arg is SyntaxNode node)
            {
                if (node.SourceFile == null)
                    return;

                throw new ArgumentNotPersisitentException(
                    $"The given syntax node {arg.GetType()} is not persistent, because it's part of a source file.");
            }

            // delegates without closures are fine
            if (arg is Delegate del && !ClosureChecker.HasClosure(del))
                return;

            throw new ArgumentNotPersisitentException($"The given {arg.GetType()} is not persistent.");
        }
    }

    internal class ArgumentNotPersisitentException : ArgumentException
    {
        public ArgumentNotPersisitentException(string message) : base(message) { }
    }
}