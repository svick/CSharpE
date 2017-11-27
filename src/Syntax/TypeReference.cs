using System;
using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSharpE.Syntax
{
    public abstract class TypeReference : ISyntaxWrapper<TypeSyntax>
    {
        public static implicit operator TypeReference(Type type) => new NamedTypeReference(type);

        TypeSyntax ISyntaxWrapper<TypeSyntax>.GetWrapped(WrapperContext context) => GetWrapped(context);

        internal TypeSyntax GetWrapped(WrapperContext context) => GetWrappedImpl(context);

        protected abstract TypeSyntax GetWrappedImpl(WrapperContext context);
    }
}