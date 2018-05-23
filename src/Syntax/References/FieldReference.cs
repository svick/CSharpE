using System;
using CSharpE.Syntax.Internals;

namespace CSharpE.Syntax
{
    public sealed class FieldReference : MemberReference, IPersistent
    {
        public FieldReference(NamedTypeReference containingType, string name, bool isStatic)
            : base(containingType, name, isStatic) { }

        public static implicit operator MemberAccessExpression(FieldReference fieldReference) =>
            new MemberAccessExpression(fieldReference);

        void IPersistent.Persist()
        {
            ((IPersistent)ContainingType).Persist();
        }
    }

    public abstract class MemberReference : IEquatable<MemberReference>
    {
        public NamedTypeReference ContainingType { get; }
        public string Name { get; }
        public bool IsStatic { get; }

        protected MemberReference(NamedTypeReference containingType, string name, bool isStatic)
        {
            ContainingType = containingType ?? throw new ArgumentNullException(nameof(containingType));
            Name = name ?? throw new ArgumentNullException(nameof(name));
            IsStatic = isStatic;
        }

        public bool Equals(MemberReference other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            if (other.GetType() != this.GetType()) return false;

            return ContainingType.Equals(other.ContainingType) && StringComparer.Ordinal.Equals(Name, other.Name) &&
                   IsStatic == other.IsStatic;
        }

        public override bool Equals(object obj) => Equals(obj as MemberReference);

        public override int GetHashCode() =>
            HashCode.Combine(ContainingType, StringComparer.Ordinal.GetHashCode(Name), IsStatic);
    }
}