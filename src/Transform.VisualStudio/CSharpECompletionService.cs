using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Completion;
using Microsoft.CodeAnalysis.Host;
using Microsoft.CodeAnalysis.Host.Mef;
using Microsoft.CodeAnalysis.Options;
using Microsoft.CodeAnalysis.Text;
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
    internal sealed class CSharpECompletionServiceFactory : ILanguageServiceFactory
    {
        private readonly ExportProvider exportProvider;

        [ImportingConstructor]
        public CSharpECompletionServiceFactory(ExportProvider exportProvider) => this.exportProvider = exportProvider;

        public ILanguageService CreateLanguageService(HostLanguageServices languageServices) =>
            new CSharpECompletionService(exportProvider, languageServices);
    }

    internal sealed class CSharpECompletionService : CompletionService
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

        public override async Task<CompletionList> GetCompletionsAsync(
            Document document, int caretPosition, CompletionTrigger trigger = default,
            ImmutableHashSet<string> roles = null, OptionSet options = null,
            CancellationToken cancellationToken = default)
        {
            var completionList = await cSharpCompletionService.GetCompletionsAsync(
                document, caretPosition, trigger, roles, options, cancellationToken);

            return completionList.WithItems(completionList.Items.Insert(0, CompletionItem.Create("CSHARPEEE")));
        }

        public override ImmutableArray<CompletionItem> FilterItems(
            Document document, ImmutableArray<CompletionItem> items, string filterText) =>
            cSharpCompletionService.FilterItems(document, items, filterText);

        public override Task<CompletionChange> GetChangeAsync(
            Document document, CompletionItem item, char? commitCharacter = null,
            CancellationToken cancellationToken = default) =>
            cSharpCompletionService.GetChangeAsync(document, item, commitCharacter, cancellationToken);

        public override TextSpan GetDefaultCompletionListSpan(SourceText text, int caretPosition) =>
            cSharpCompletionService.GetDefaultCompletionListSpan(text, caretPosition);

        public override Task<CompletionDescription> GetDescriptionAsync(
            Document document, CompletionItem item, CancellationToken cancellationToken = default) =>
            cSharpCompletionService.GetDescriptionAsync(document, item, cancellationToken);

        public override CompletionRules GetRules() => cSharpCompletionService.GetRules();

        public override bool ShouldTriggerCompletion(
            SourceText text, int caretPosition, CompletionTrigger trigger, ImmutableHashSet<string> roles = null,
            OptionSet options = null) =>
            cSharpCompletionService.ShouldTriggerCompletion(text, caretPosition, trigger, roles, options);
    }
}
