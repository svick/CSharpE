using System;
using System.Reflection;
using Microsoft.CodeAnalysis;

namespace CSharpE.Syntax
{
    public sealed class AssemblyReference : LibraryReference, IEquatable<AssemblyReference>
    {
        public string Path { get; }

        public AssemblyReference(string path) => Path = path ?? throw new ArgumentNullException(nameof(path));

        public AssemblyReference(Assembly assembly) : this(assembly.Location) { }

        public AssemblyReference(Type assemblyRepresentativeType)
            : this(assemblyRepresentativeType.Assembly) { }

        public override MetadataReference GetMetadataReference() =>
            MetadataReference.CreateFromFile(Path);

        public bool Equals(AssemblyReference other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(Path, other.Path);
        }

        public override bool Equals(object obj) => Equals(obj as AssemblyReference);

        public override int GetHashCode() => Path.GetHashCode();
    }
}