﻿using System;
using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Host;
using Microsoft.CodeAnalysis.Host.Mef;
using Microsoft.VisualStudio.Composition;
using RoslynCompilation = Microsoft.CodeAnalysis.Compilation;
using static CSharpE.Transform.VisualStudio.Wrapping;

namespace CSharpE.Transform.VisualStudio
{
    [ExportLanguageServiceFactory(typeof(ICompilationFactoryService), LanguageNames.CSharp, ServiceLayer.Host), Shared]
    internal sealed class CompilationFactoryServiceFactory : ILanguageServiceFactory
    {
        private readonly ExportProvider exportProvider;

        [ImportingConstructor]
        public CompilationFactoryServiceFactory(ExportProvider exportProvider) => this.exportProvider = exportProvider;

        public ILanguageService CreateLanguageService(HostLanguageServices languageServices) =>
            new CompilationFactoryService(exportProvider, languageServices);
    }

    internal sealed class CompilationFactoryService : ICompilationFactoryService
    {
        private readonly ICompilationFactoryService roslynCompilationFactoryService;

        public CompilationFactoryService(ExportProvider exportProvider, HostLanguageServices languageServices) =>
            roslynCompilationFactoryService =
                LanguageServices.GetCSharpService<ICompilationFactoryService>(exportProvider, languageServices);

        public RoslynCompilation CreateCompilation(string assemblyName, CompilationOptions options)
        {
            return Wrap(roslynCompilationFactoryService.CreateCompilation(assemblyName, options));
        }

        public RoslynCompilation CreateSubmissionCompilation(string assemblyName, CompilationOptions options, Type hostObjectType)
        {
            return Wrap(roslynCompilationFactoryService.CreateSubmissionCompilation(assemblyName, options, hostObjectType));
        }

        public RoslynCompilation GetCompilationFromCompilationReference(MetadataReference reference)
        {
            return Wrap(roslynCompilationFactoryService.GetCompilationFromCompilationReference(reference));
        }

        public CompilationOptions GetDefaultCompilationOptions()
        {
            return roslynCompilationFactoryService.GetDefaultCompilationOptions();
        }

        public bool IsCompilationReference(MetadataReference reference)
        {
            return roslynCompilationFactoryService.IsCompilationReference(reference);
        }
    }
}