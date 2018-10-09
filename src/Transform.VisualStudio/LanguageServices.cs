using System;
using System.Linq;
using Microsoft.CodeAnalysis.Host;
using Microsoft.CodeAnalysis.Host.Mef;
using Microsoft.VisualStudio.Composition;
using static Microsoft.CodeAnalysis.LanguageNames;

namespace CSharpE.Transform.VisualStudio
{
    internal static class LanguageServices
    {
        public static T GetCSharpService<T>(ExportProvider exportProvider, HostLanguageServices languageServices)
        {
            // based on code from Microsoft.CodeAnalysis.Host.Mef.MefLanguageServices
            return (T)exportProvider.GetExports<ILanguageService, LanguageServiceMetadata>()
                .Concat(exportProvider.GetExports<ILanguageServiceFactory, LanguageServiceMetadata>()
                    .Select(lz => new Lazy<ILanguageService, LanguageServiceMetadata>(() => lz.Value.CreateLanguageService(languageServices), lz.Metadata)))
                .Single(
                    lz => lz.Metadata.Language == CSharp &&
                          lz.Metadata.ServiceType == typeof(T).AssemblyQualifiedName &&
                          lz.Metadata.Layer == ServiceLayer.Default).Value;
        }
    }
}