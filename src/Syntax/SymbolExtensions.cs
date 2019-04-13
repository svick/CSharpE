using System;
using Microsoft.CodeAnalysis;

namespace CSharpE.Syntax
{
    public static class SymbolExtensions
    {
        public static bool HasBaseClass<T>(this INamedTypeSymbol symbol) => symbol.HasBaseClass(typeof(T).FullName);

        public static bool HasBaseClass(this INamedTypeSymbol symbol, string baseClassFullName)
        {
            var current = symbol;

            while (current != null)
            {
                if (current.ToDisplayString() == baseClassFullName)
                    return true;

                current = current.BaseType;
            }

            return false;
        }
    }
}