using System;

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

    public abstract class MemberReference
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
    }
}