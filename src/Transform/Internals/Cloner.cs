using System;
using CSharpE.Syntax;

namespace CSharpE.Transform.Internals
{
    internal static class Cloner
    {
        private static bool IsImmutable<T>(T arg)
        {
            // TODO: detect other immutable types
            return arg is Unit || arg is int || arg is string;
        }        

        
        public static T DeepClone<T>(T input)
        {
            if (IsImmutable(input))
                return input;

            if (input is SyntaxNode syntaxNode)
                return (T) (object) syntaxNode.Clone();

            if (TupleHandler.IsTuple(input))
                return TupleHandler.DeepClone(input);
            
            throw new InvalidOperationException($"The object {input} has to be cloneable.");
        }
    }
}