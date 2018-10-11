using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Host;
using Microsoft.CodeAnalysis.Host.Mef;
using System.Composition;
using Microsoft.VisualStudio.Composition;
using Microsoft.CodeAnalysis.Text;
using Roslyn.Utilities;
using System.IO;
using System.Text;
using System.Threading;
using RoslynSyntaxTree = Microsoft.CodeAnalysis.SyntaxTree;

namespace CSharpE.Transform.VisualStudio
{
    //[ExportLanguageServiceFactory(typeof(ISyntaxTreeFactoryService), LanguageNames.CSharp, ServiceLayer.Host), Shared]
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

        private static SyntaxTree Wrap(RoslynSyntaxTree roslynSyntaxTree) => new SyntaxTree(roslynSyntaxTree);

        public bool CanCreateRecoverableTree(SyntaxNode root)
        {
            return roslynSyntaxTreeFactoryService.CanCreateRecoverableTree(root);
        }

        public RoslynSyntaxTree CreateRecoverableTree(
            ProjectId cacheKey, string filePath, ParseOptions options, ValueSource<TextAndVersion> text,
            Encoding encoding, SyntaxNode root)
        {
            return roslynSyntaxTreeFactoryService.CreateRecoverableTree(
                cacheKey, filePath, options, text, encoding, root);
        }

        public RoslynSyntaxTree CreateSyntaxTree(string filePath, ParseOptions options, Encoding encoding, SyntaxNode root) =>
            Wrap(roslynSyntaxTreeFactoryService.CreateSyntaxTree(filePath, options, encoding, root));

        public SyntaxNode DeserializeNodeFrom(Stream stream, CancellationToken cancellationToken)
        {
            return roslynSyntaxTreeFactoryService.DeserializeNodeFrom(stream, cancellationToken);
        }

        public ParseOptions GetDefaultParseOptions()
        {
            return roslynSyntaxTreeFactoryService.GetDefaultParseOptions();
        }

        public ParseOptions GetDefaultParseOptionsWithLatestLanguageVersion()
        {
            return roslynSyntaxTreeFactoryService.GetDefaultParseOptionsWithLatestLanguageVersion();
        }

        public RoslynSyntaxTree ParseSyntaxTree(
            string filePath, ParseOptions options, SourceText text, CancellationToken cancellationToken) =>
            Wrap(roslynSyntaxTreeFactoryService.ParseSyntaxTree(filePath, options, text, cancellationToken));
    }
}
