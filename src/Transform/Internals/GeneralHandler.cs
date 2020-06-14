using System;
using System.Collections.Generic;
using CSharpE.Syntax;

namespace CSharpE.Transform.Internals
{
    internal static class GeneralHandler
    {
        public static void ThrowIfNotTrackable<T>(T arg)
        {
            if (IsImmutable(arg))
                return;

            if (arg is SyntaxNode)
                return;

            if (CollectionHandler.IsCollection(arg))
            {
                CollectionHandler.ThrowIfNotTrackable(arg);
                
                return;
            }

            if (TupleHandler.IsTuple(arg))
            {
                TupleHandler.ThrowIfNotTrackable(arg);

                return;
            }

            throw new ArgumentNotPersistentException($"The given {arg.GetType()} cannot be tracked.");
        }

        public static bool IsImmutable<T>(T arg)
        {
            if (arg == null)
                return true;

            // TODO: detect other immutable types

            if (arg is Unit || arg is string || arg.GetType().IsPrimitive)
                return true;

            // delegates without closures are considered immutable
            if (arg is Delegate del && !ClosureChecker.HasClosure(del))
                return true;

            if (TupleHandler.IsTuple(arg))
                return TupleHandler.IsImmutable(arg);

            return false;
        }

        public static T DeepClone<T>(T input)
        {
            if (IsImmutable(input))
                return input;

            if (input is SyntaxNode syntaxNode)
                return (T)(object)syntaxNode.Clone();

            if (CollectionHandler.IsCollection(input))
                return CollectionHandler.DeepClone(input);

            if (TupleHandler.IsTuple(input))
                return TupleHandler.DeepClone(input);

            throw new InvalidOperationException($"The object {input} cannot be cloned.");
        }

        public static bool Equals<T>(T arg1, T arg2)
        {
            if (arg1 is SyntaxNode node1 && arg2 is SyntaxNode node2)
                return SyntaxNode.AreEquivalent(node1, node2);

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
