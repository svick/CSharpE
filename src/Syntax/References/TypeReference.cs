using System;
using System.Text;
using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSharpE.Syntax
{
    public abstract class TypeReference : SyntaxNode, ISyntaxWrapper<TypeSyntax>
    {
        public static implicit operator TypeReference(Type type) => type == null ? null : new NamedTypeReference(type);

        TypeSyntax ISyntaxWrapper<TypeSyntax>.GetWrapped() => GetWrapped();

        internal TypeSyntax GetWrapped() => GetWrappedImpl();

        protected abstract TypeSyntax GetWrappedImpl();

        internal abstract StringBuilder ComputeFullName(StringBuilder stringBuilder);
    }
}