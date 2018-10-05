using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Completion;
using Microsoft.CodeAnalysis.Host;
using Microsoft.CodeAnalysis.Host.Mef;
using Microsoft.CodeAnalysis.Options;
using Microsoft.VisualStudio.Composition;
using System;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static Microsoft.CodeAnalysis.LanguageNames;

namespace CSharpE.Transform.VisualStudio
{
    [ExportLanguageServiceFactory(typeof(CompletionService), CSharp, ServiceLayer.Host)]
    internal class CSharpECompletionServiceFactory : ILanguageServiceFactory
    {
        private readonly ExportProvider exportProvider;

        [ImportingConstructor]
        public CSharpECompletionServiceFactory(ExportProvider exportProvider) => this.exportProvider = exportProvider;

        public ILanguageService CreateLanguageService(HostLanguageServices languageServices) =>
            new CSharpECompletionService(exportProvider, languageServices);
    }

    class CSharpECompletionService : CompletionService
    {
        private readonly CompletionService cSharpCompletionService;

        public CSharpECompletionService(ExportProvider exportProvider, HostLanguageServices languageServices)
        {
            // based on code from Microsoft.CodeAnalysis.Host.Mef.MefLanguageServices
            cSharpCompletionService = (CompletionService)exportProvider
                .GetExports<ILanguageServiceFactory, LanguageServiceMetadata>()
                .Select(
                    lz => new Lazy<ILanguageService, LanguageServiceMetadata>(
                        () => lz.Value.CreateLanguageService(languageServices), lz.Metadata))
                .Single(
                    lz => lz.Metadata.Language == CSharp &&
                          lz.Metadata.ServiceType == typeof(CompletionService).AssemblyQualifiedName &&
                          lz.Metadata.Layer == ServiceLayer.Default).Value;
        }

        public override string Language => CSharp;

        public override async Task<CompletionList> GetCompletionsAsync(Document document, int caretPosition, CompletionTrigger trigger = default,
            ImmutableHashSet<string> roles = null, OptionSet options = null, CancellationToken cancellationToken = default)
        {
            var completionList = await cSharpCompletionService.GetCompletionsAsync(
                document, caretPosition, trigger, roles, options, cancellationToken);

            return completionList.WithItems(completionList.Items.Insert(0, CompletionItem.Create("CSHARPEEE")));
        }
    }
}
