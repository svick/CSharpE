using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Options;
using Microsoft.CodeAnalysis.SolutionCrawler;
using Microsoft.VisualStudio.Composition;

namespace CSharpE.Transform.VisualStudio
{
    // "CSharpE" is a sentinel value so that it's possible to identify this type from metadata
    [ExportIncrementalAnalyzerProvider(true, WellKnownSolutionCrawlerAnalyzers.Diagnostic, workspaceKinds: new[] { WorkspaceKind.Host, nameof(CSharpE) })]
    internal class DefaultDiagnosticAnalyzerService : IIncrementalAnalyzerProvider//, IDiagnosticUpdateSource
    {
        private readonly Lazy<IIncrementalAnalyzerProvider> lazyRoslynAnalyzerProvider;

        [ImportingConstructor]
        public DefaultDiagnosticAnalyzerService(ExportProvider exportProvider) =>
            lazyRoslynAnalyzerProvider = exportProvider.GetExports<IIncrementalAnalyzerProvider, IncrementalAnalyzerProviderMetadata>()
                .Single(lazy => lazy.Metadata.Name == WellKnownSolutionCrawlerAnalyzers.Diagnostic && lazy.Metadata.WorkspaceKinds != null &&
                    lazy.Metadata.WorkspaceKinds.Contains(WorkspaceKind.Host) && !lazy.Metadata.WorkspaceKinds.Contains(nameof(CSharpE)));

        public IIncrementalAnalyzer CreateIncrementalAnalyzer(Workspace workspace) => new IncrementalAnalyzer(lazyRoslynAnalyzerProvider.Value.CreateIncrementalAnalyzer(workspace));

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
}
