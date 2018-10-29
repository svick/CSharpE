using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Completion;
using Microsoft.CodeAnalysis.Host;
using Microsoft.CodeAnalysis.Host.Mef;
using Microsoft.CodeAnalysis.Options;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.Composition;
using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using static Microsoft.CodeAnalysis.LanguageNames;
using RoslynCompletionService = Microsoft.CodeAnalysis.Completion.CompletionService;

namespace CSharpE.Transform.VisualStudio
{
    [ExportLanguageServiceFactory(typeof(RoslynCompletionService), CSharp, ServiceLayer.Host)]
    internal sealed class CompletionServiceFactory : ILanguageServiceFactory
    {
        private readonly ExportProvider exportProvider;

        [ImportingConstructor]
        public CompletionServiceFactory(ExportProvider exportProvider) => this.exportProvider = exportProvider;

        public ILanguageService CreateLanguageService(HostLanguageServices languageServices) =>
            new CompletionService(exportProvider, languageServices);
    }

    internal sealed class CompletionService : RoslynCompletionService
    {
        private readonly RoslynCompletionService roslynCompletionService;

        public CompletionService(ExportProvider exportProvider, HostLanguageServices languageServices) =>
            roslynCompletionService =
                LanguageServices.GetCSharpService<RoslynCompletionService>(exportProvider, languageServices);

        public override string Language => CSharp;

        public override async Task<CompletionList> GetCompletionsAsync(
            Document document, int caretPosition, CompletionTrigger trigger = default,
            ImmutableHashSet<string> roles = null, OptionSet options = null,
            CancellationToken cancellationToken = default)
        {
            // TODO: cancellation
           var (adjustedDocument, adjustedPosition) = await ProjectInfo.Get(document.Project).Adjust(document, caretPosition);

            if (adjustedPosition == null)
            {
                // TODO?
                return CompletionList.Empty;
            }

            return await roslynCompletionService.GetCompletionsAsync(
                adjustedDocument, adjustedPosition.Value, trigger, roles, options, cancellationToken);
        }

        public override ImmutableArray<CompletionItem> FilterItems(
            Document document, ImmutableArray<CompletionItem> items, string filterText) =>
            roslynCompletionService.FilterItems(document, items, filterText);

        public override Task<CompletionChange> GetChangeAsync(
            Document document, CompletionItem item, char? commitCharacter = null,
            CancellationToken cancellationToken = default) =>
            roslynCompletionService.GetChangeAsync(document, item, commitCharacter, cancellationToken);

        public override TextSpan GetDefaultCompletionListSpan(SourceText text, int caretPosition) =>
            roslynCompletionService.GetDefaultCompletionListSpan(text, caretPosition);

        public override Task<CompletionDescription> GetDescriptionAsync(
            Document document, CompletionItem item, CancellationToken cancellationToken = default) =>
            roslynCompletionService.GetDescriptionAsync(document, item, cancellationToken);

        public override CompletionRules GetRules() => roslynCompletionService.GetRules();

        public override bool ShouldTriggerCompletion(
            SourceText text, int caretPosition, CompletionTrigger trigger, ImmutableHashSet<string> roles = null,
            OptionSet options = null) =>
            roslynCompletionService.ShouldTriggerCompletion(text, caretPosition, trigger, roles, options);
    }
}
