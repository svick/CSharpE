using System;
using System.Collections.Generic;
using CSharpE.Syntax;

namespace CSharpE.Transform.Internals
{
    internal static class GeneralHandler
    {
        public static void ThrowIfNotPersistent<T>(T arg)
        {
            if (arg == null)
                return;

            if (IsImmutable(arg))
                return;

            // disconnected syntax nodes are fine
            if (arg is SyntaxNode node)
            {
                if (node.SourceFile == null)
                    return;

                throw new ArgumentNotPersistentException(
                    $"The given syntax node {arg.GetType()} is not persistent, because it's part of a source file.");
            }

            if (CollectionHandler.IsCollection(arg))
            {
                CollectionHandler.ThrowIfNotPersistent(arg);
                
                return;
            }

            if (TupleHandler.IsTuple(arg))
            {
                TupleHandler.ThrowIfNotPersistent(arg);

                return;
            }

            throw new ArgumentNotPersistentException($"The given {arg.GetType()} is not persistent.");
        }

        private static bool IsImmutable<T>(T arg)
        {
            // TODO: detect other immutable types

            if (arg is Unit || arg is bool || arg is int || arg is string)
                return true;

            // delegates without closures are considered immutable
            if (arg is Delegate del && !ClosureChecker.HasClosure(del))
                return true;

            return false;
        }

        public static T DeepClone<T>(T input)
        {
            if (IsImmutable(input))
                return input;

            if (input is SyntaxNode syntaxNode)
                return (T)(object)syntaxNode.Clone();

            if (TupleHandler.IsTuple(input))
                return TupleHandler.DeepClone(input);

            if (CollectionHandler.IsCollection(input))
                return CollectionHandler.DeepClone(input);

            throw new InvalidOperationException($"The object {input} has to be cloneable.");
        }

        public static bool Equals<T>(T arg1, T arg2)
        {
            if (CollectionHandler.IsCollection(arg1) && CollectionHandler.IsCollection(arg2))
                return CollectionHandler.Equals(arg1, arg2);

            if (TupleHandler.IsTuple(arg1) && TupleHandler.IsTuple(arg2))
                return TupleHandler.Equals(arg1, arg2);

            return EqualityComparer<T>.Default.Equals(arg1, arg2);
        }
    }

    internal class ArgumentNotPersistentException : ArgumentException
    {
        public ArgumentNotPersistentException(string message) : base(message) { }
    }
}
