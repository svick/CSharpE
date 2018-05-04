using System;
using System.Text;
using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSharpE.Syntax
{
    public abstract class TypeReference : SyntaxNode, ISyntaxWrapper<TypeSyntax>
    {
        public static implicit operator TypeReference(Type type) => type == null ? null : new NamedTypeReference(type);

        TypeSyntax ISyntaxWrapper<TypeSyntax>.GetWrapped(WrapperContext context) => GetWrapped(context);

        internal TypeSyntax GetWrapped(WrapperContext context) => GetWrappedImpl(context);

        protected abstract TypeSyntax GetWrappedImpl(WrapperContext context);

        internal abstract StringBuilder ComputeFullName(StringBuilder stringBuilder);
    }
}