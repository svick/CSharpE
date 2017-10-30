using System;
using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSharpE.Syntax
{
    public abstract class TypeReference : ISyntaxWrapper<TypeSyntax>
    {
        public static implicit operator TypeReference(Type type) => new NamedTypeReference(type);

        TypeSyntax ISyntaxWrapper<TypeSyntax>.GetWrapped() => GetWrapped();

        internal TypeSyntax GetWrapped() => GetWrappedImpl();

        protected abstract TypeSyntax GetWrappedImpl();
    }
}