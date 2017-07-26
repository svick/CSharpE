using Microsoft.CodeAnalysis;

namespace CSharpE.Syntax
{
    // TODO: "reference" is too general, is there a more specific name that can be used?
    public abstract class Reference
    {
        internal abstract MetadataReference GetMetadataReference();
    }
}