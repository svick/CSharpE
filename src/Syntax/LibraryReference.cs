using Microsoft.CodeAnalysis;

namespace CSharpE.Syntax
{
    public abstract class LibraryReference
    {
        internal abstract MetadataReference GetMetadataReference();
    }
}