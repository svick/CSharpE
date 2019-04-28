using System;
using System.Text;
using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSharpE.Syntax
{
    public abstract class TypeReference : Expression, ISyntaxWrapper<TypeSyntax>, IEquatable<TypeReference>
    {
        private protected TypeReference() { }

        private protected TypeReference(TypeSyntax syntax) : base(syntax) { }

        public static implicit operator TypeReference(Type type) => type == null ? null : new NamedTypeReference(type);

        TypeSyntax ISyntaxWrapper<TypeSyntax>.GetWrapped(ref bool? changed) => GetWrappedType(ref changed);

        private protected sealed override ExpressionSyntax GetWrappedExpression(ref bool? changed) => GetWrappedType(ref changed);

        internal TypeSyntax GetWrapped(ref bool? changed) => GetWrappedType(ref changed);

        private protected abstract TypeSyntax GetWrappedType(ref bool? changed);

        internal abstract StringBuilder ComputeFullName(StringBuilder stringBuilder);

        public string FullName => ComputeFullName(new StringBuilder()).ToString();

        public sealed override bool Equals(object obj) => Equals(obj as TypeReference);

        public abstract bool Equals(TypeReference other);

        public abstract override int GetHashCode();
    }
}