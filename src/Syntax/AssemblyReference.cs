using System;
using System.Reflection;
using Microsoft.CodeAnalysis;

namespace CSharpE.Syntax
{
    public class AssemblyReference : LibraryReference, IEquatable<AssemblyReference>
    {
        public Assembly Assembly { get; }

        public AssemblyReference(Assembly assembly) =>
            Assembly = assembly ?? throw new ArgumentNullException(nameof(assembly));

        public AssemblyReference(Type assemblyRepresentativeType)
            : this(assemblyRepresentativeType.Assembly)
        { }

        internal override MetadataReference GetMetadataReference() =>
            MetadataReference.CreateFromFile(Assembly.Location);

        public bool Equals(AssemblyReference other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(Assembly, other.Assembly);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((AssemblyReference) obj);
        }

        public override int GetHashCode() => Assembly.GetHashCode();
    }
}