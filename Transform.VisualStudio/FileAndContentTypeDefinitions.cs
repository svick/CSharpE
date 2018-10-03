using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Utilities;

namespace CSharpE.Transform.VisualStudio
{
    internal static class FileAndContentTypeDefinitions
    {
        [Export]
        [Name("CSharpE")]
        [BaseDefinition("CSharp")]
        internal static ContentTypeDefinition cSharpEContentTypeDefinition;

        [Export]
        [FileExtension(".cse")]
        [ContentType("CSharpE")]
        internal static FileExtensionToContentTypeDefinition cSharpEFileExtensionDefinition;
    }
}
