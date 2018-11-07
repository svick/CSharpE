using System;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Options;
using Microsoft.CodeAnalysis.Shared.TestHooks;
using Microsoft.CodeAnalysis.SolutionCrawler;
using Microsoft.VisualStudio.Composition;

namespace CSharpE.Transform.VisualStudio
{
    [ExportIncrementalAnalyzerProvider(true, WellKnownSolutionCrawlerAnalyzers.Diagnostic, workspaceKinds: new[] { WorkspaceKind.Host })]
    internal class DiagnosticAnalyzerService : IIncrementalAnalyzerProvider//, IDiagnosticUpdateSource
    {
        private readonly DiagnosticAnalyzerService2 diagnosticAnalyzerService;

        [ImportingConstructor]
        public DiagnosticAnalyzerService(DiagnosticAnalyzerService2 diagnosticAnalyzerService) => 
            this.diagnosticAnalyzerService = diagnosticAnalyzerService;

        public IIncrementalAnalyzer CreateIncrementalAnalyzer(Workspace workspace) =>
            new IncrementalAnalyzer(diagnosticAnalyzerService.CreateIncrementalAnalyzer(workspace));

        class IncrementalAnalyzer : IIncrementalAnalyzer
        {
            private readonly IIncrementalAnalyzer roslynIncrementalAnalyzer;

            public IncrementalAnalyzer(IIncrementalAnalyzer roslynIncrementalAnalyzer) => this.roslynIncrementalAnalyzer = roslynIncrementalAnalyzer;

            public async Task AnalyzeDocumentAsync(Document document, SyntaxNode bodyOpt, InvocationReasons reasons, CancellationToken cancellationToken) =>
                // TODO: is always passing null for bodyOpt correct?
                await roslynIncrementalAnalyzer.AnalyzeDocumentAsync(await ProjectInfo.Get(document.Project).Adjust(document), null, reasons, cancellationToken);

            public async Task AnalyzeProjectAsync(Project project, bool semanticsChanged, InvocationReasons reasons, CancellationToken cancellationToken) =>
                // TODO: what if semantics changed only in transformed project?
                await roslynIncrementalAnalyzer.AnalyzeProjectAsync(await ProjectInfo.Get(project).Adjust(project), semanticsChanged, reasons, cancellationToken);

            public async Task AnalyzeSyntaxAsync(Document document, InvocationReasons reasons, CancellationToken cancellationToken) =>
                await roslynIncrementalAnalyzer.AnalyzeSyntaxAsync(await ProjectInfo.Get(document.Project).Adjust(document), reasons, cancellationToken);

            public Task DocumentCloseAsync(Document document, CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }

            public async Task DocumentOpenAsync(Document document, CancellationToken cancellationToken) =>
                await roslynIncrementalAnalyzer.DocumentOpenAsync(await ProjectInfo.Get(document.Project).Adjust(document), cancellationToken);

            public Task DocumentResetAsync(Document document, CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }

            public bool NeedsReanalysisOnOptionChanged(object sender, OptionChangedEventArgs e) => roslynIncrementalAnalyzer.NeedsReanalysisOnOptionChanged(sender, e);

            public Task NewSolutionSnapshotAsync(Solution solution, CancellationToken cancellationToken) =>
                roslynIncrementalAnalyzer.NewSolutionSnapshotAsync(solution, cancellationToken);

            public void RemoveDocument(DocumentId documentId)
            {
                throw new NotImplementedException();
            }

            public void RemoveProject(ProjectId projectId)
            {
                throw new NotImplementedException();
            }
        }
    }

    [Export(typeof(DiagnosticAnalyzerService2))]
    internal class DiagnosticAnalyzerService2 : Microsoft.CodeAnalysis.Diagnostics.DiagnosticAnalyzerService, IDiagnosticUpdateSource
    {
        [ImportingConstructor]
        public DiagnosticAnalyzerService2(
            IDiagnosticUpdateSourceRegistrationService registrationService,
            IAsynchronousOperationListenerProvider listenerProvider,
            PrimaryWorkspace primaryWorkspace,
            [Import(AllowDefault = true)]IWorkspaceDiagnosticAnalyzerProviderService diagnosticAnalyzerProviderService,
            [Import(AllowDefault = true)]AbstractHostDiagnosticUpdateSource hostDiagnosticUpdateSource)
            : base(registrationService, listenerProvider, primaryWorkspace, diagnosticAnalyzerProviderService, hostDiagnosticUpdateSource)
        { }

        private DiagnosticData Adjust(DiagnosticData data) => throw new NotImplementedException();

        private DiagnosticsUpdatedArgs Adjust(DiagnosticsUpdatedArgs args)
        {
            if (args.Diagnostics.IsEmpty)
                return args;

            return DiagnosticsUpdatedArgs.DiagnosticsCreated(
                args.Id, args.Workspace, args.Solution, args.ProjectId, args.DocumentId,
                ImmutableArray.CreateRange(args.Diagnostics.Select(Adjust)));
        }

        public new event EventHandler<DiagnosticsUpdatedArgs> DiagnosticsUpdated
        {
            add => base.DiagnosticsUpdated += (s, e) => value(s, Adjust(e));
            remove => throw new NotImplementedException();
        }
    }
}
