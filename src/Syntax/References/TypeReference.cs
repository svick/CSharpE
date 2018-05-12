using System;
using System.Text;
using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSharpE.Syntax
{
    public abstract class TypeReference : SyntaxNode, ISyntaxWrapper2<TypeSyntax>
    {
        public static implicit operator TypeReference(Type type) => type == null ? null : new NamedTypeReference(type);

        TypeSyntax ISyntaxWrapper2<TypeSyntax>.GetWrapped(ref bool changed) => GetWrapped(ref changed);

        internal TypeSyntax GetWrapped(ref bool changed) => GetWrappedImpl(ref changed);

        protected abstract TypeSyntax GetWrappedImpl(ref bool changed);

        internal abstract StringBuilder ComputeFullName(StringBuilder stringBuilder);
    }
}