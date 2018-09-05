using System;
using CSharpE.Syntax;

namespace CSharpE.Transform.Internals
{
    internal static class Cloner
    {
        private static bool IsImmutable<T>()
        {
            // TODO: detect other immutable types
            return typeof(T).IsValueType || typeof(T) == typeof(string);
        }
        
        public static T DeepClone<T>(T input)
        {
            if (IsImmutable<T>())
                return input;

            if (input is SyntaxNode syntaxNode)
                return (T) (object) syntaxNode.Clone();
            
            throw new InvalidOperationException($"The object {input} has to be cloneable.");
        }
    }
}