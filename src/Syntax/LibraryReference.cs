using Microsoft.CodeAnalysis;

namespace CSharpE.Syntax
{
    public abstract class LibraryReference
    {
        public abstract MetadataReference GetMetadataReference();
    }
}