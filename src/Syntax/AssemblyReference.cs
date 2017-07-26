using System;
using System.Reflection;
using Microsoft.CodeAnalysis;

namespace CSharpE.Syntax
{
    public class AssemblyReference : Reference
    {
        public Assembly Assembly { get; }

        public AssemblyReference(Assembly assembly) => Assembly = assembly;

        public AssemblyReference(Type assemblyRepresentativeType)
            : this(assemblyRepresentativeType.GetTypeInfo().Assembly)
        { }

        internal override MetadataReference GetMetadataReference() =>
            MetadataReference.CreateFromFile(Assembly.Location);
    }
}