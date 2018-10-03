using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Completion;
using Microsoft.CodeAnalysis.Host;
using Microsoft.CodeAnalysis.Host.Mef;
using Microsoft.CodeAnalysis.Options;
using System;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;

namespace Transform.VisualStudio
{
    //[ExportLanguageServiceFactory(typeof(CompletionService), "CSharpE"), Shared]
    internal class CSharpECompletionServiceFactory : ILanguageServiceFactory
    {
        public ILanguageService CreateLanguageService(HostLanguageServices languageServices)
        {
            throw new Exception();
            //return new CSharpCompletionService(languageServices.WorkspaceServices.Workspace);
        }
    }

    [ExportLanguageService(typeof(CompletionService), "CSharpE")]
    class CSharpECompletionService : CompletionService
    {
        private readonly CompletionService cSharpCompletionService;

        public CSharpECompletionService(HostWorkspaceServices workspaceServices) => cSharpCompletionService =
            workspaceServices.GetLanguageServices(LanguageNames.CSharp).GetService<CompletionService>();

        public override string Language => "CSharpE";

        public override Task<CompletionList> GetCompletionsAsync(Document document, int caretPosition, CompletionTrigger trigger = default,
            ImmutableHashSet<string> roles = null, OptionSet options = null, CancellationToken cancellationToken = default)
        {
            return cSharpCompletionService.GetCompletionsAsync(document, caretPosition, trigger, roles, options, cancellationToken);
        }
    }
}
