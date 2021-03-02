using System.Composition;
using System.IO;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Host;
using Microsoft.CodeAnalysis.Host.Mef;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.Composition;
using Roslyn.Utilities;
using RoslynSyntaxTree = Microsoft.CodeAnalysis.SyntaxTree;
using static CSharpE.Transform.VisualStudio.SyntaxTree;

namespace CSharpE.Transform.VisualStudio
{
    [ExportLanguageServiceFactory(typeof(ISyntaxTreeFactoryService), LanguageNames.CSharp, ServiceLayer.Host), Shared]
    internal sealed class SyntaxTreeFactoryServiceFactory : ILanguageServiceFactory
    {
        private readonly ExportProvider exportProvider;

        [ImportingConstructor]
        public SyntaxTreeFactoryServiceFactory(ExportProvider exportProvider) => this.exportProvider = exportProvider;

        public ILanguageService CreateLanguageService(HostLanguageServices languageServices) =>
            new SyntaxTreeFactoryService(exportProvider, languageServices);
    }

    internal sealed class SyntaxTreeFactoryService : ISyntaxTreeFactoryService
    {
        private readonly ISyntaxTreeFactoryService roslynSyntaxTreeFactoryService;

        public SyntaxTreeFactoryService(ExportProvider exportProvider, HostLanguageServices languageServices) =>
            roslynSyntaxTreeFactoryService =
                LanguageServices.GetCSharpService<ISyntaxTreeFactoryService>(exportProvider, languageServices);

        private static SyntaxTree Wrap(RoslynSyntaxTree tree) => new SyntaxTree((CSharpSyntaxTree)tree);

        public bool CanCreateRecoverableTree(SyntaxNode root) =>
            roslynSyntaxTreeFactoryService.CanCreateRecoverableTree(root);

        public RoslynSyntaxTree CreateRecoverableTree(
            ProjectId cacheKey, string filePath, ParseOptions options, ValueSource<TextAndVersion> text,
            Encoding encoding, SyntaxNode root)
        {
            return Wrap(roslynSyntaxTreeFactoryService.CreateRecoverableTree(
                cacheKey, filePath, options, text, encoding, root));
        }

        public RoslynSyntaxTree CreateSyntaxTree(string filePath, ParseOptions options, Encoding encoding, SyntaxNode root)
            => Wrap(roslynSyntaxTreeFactoryService.CreateSyntaxTree(filePath, options, encoding, root));

        public SyntaxNode DeserializeNodeFrom(Stream stream, CancellationToken cancellationToken) =>
            Annotate(roslynSyntaxTreeFactoryService.DeserializeNodeFrom(stream, cancellationToken));

        public ParseOptions GetDefaultParseOptions() => roslynSyntaxTreeFactoryService.GetDefaultParseOptions();

        public ParseOptions GetDefaultParseOptionsWithLatestLanguageVersion() =>
            roslynSyntaxTreeFactoryService.GetDefaultParseOptionsWithLatestLanguageVersion();

        public RoslynSyntaxTree ParseSyntaxTree(string filePath, ParseOptions options, SourceText text, CancellationToken cancellationToken)
        {
            return Wrap(
                roslynSyntaxTreeFactoryService.ParseSyntaxTree(
                    filePath, options, text, cancellationToken));
        }
    }
}
