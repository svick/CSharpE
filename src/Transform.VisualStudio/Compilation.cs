using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reflection.Metadata;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeGen;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.CodeAnalysis.Symbols;
using RoslynCompilation = Microsoft.CodeAnalysis.Compilation;
using RoslynSemanticModel = Microsoft.CodeAnalysis.SemanticModel;
using RoslynSyntaxTree = Microsoft.CodeAnalysis.SyntaxTree;
using static CSharpE.Transform.VisualStudio.Wrapping;

namespace CSharpE.Transform.VisualStudio
{
    internal sealed class Compilation : RoslynCompilation
    {
        private RoslynCompilation roslynCompilation;

        public Compilation(RoslynCompilation roslynCompilation)
            : base(roslynCompilation.AssemblyName, roslynCompilation.References.ToImmutableArray(), new Dictionary<string, string>() /* TODO? */, roslynCompilation.IsSubmission, roslynCompilation.EventQueue)
            => this.roslynCompilation = roslynCompilation;

        protected override RoslynCompilation CommonClone()
        {
            return Wrap(roslynCompilation.Clone());
        }

        protected override RoslynSemanticModel CommonGetSemanticModel(RoslynSyntaxTree syntaxTree, bool ignoreAccessibility)
        {
            var roslynModel = roslynCompilation.GetSemanticModel(Unwrap(syntaxTree), ignoreAccessibility);
            return new SemanticModel(roslynModel);
        }

        protected override INamedTypeSymbol CommonCreateErrorTypeSymbol(INamespaceOrTypeSymbol container, string name, int arity)
        {
            return roslynCompilation.CreateErrorTypeSymbol(container, name, arity);
        }

        protected override INamespaceSymbol CommonCreateErrorNamespaceSymbol(INamespaceSymbol container, string name)
        {
            return roslynCompilation.CreateErrorNamespaceSymbol(container, name);
        }

        protected override RoslynCompilation CommonWithAssemblyName(string outputName)
        {
            return Wrap(roslynCompilation.WithAssemblyName(outputName));
        }

        protected override RoslynCompilation CommonWithOptions(CompilationOptions options)
        {
            return Wrap(roslynCompilation.WithOptions(options));
        }

        protected override RoslynCompilation CommonWithScriptCompilationInfo(ScriptCompilationInfo info)
        {
            return Wrap(roslynCompilation.WithScriptCompilationInfo(info));
        }

        protected override RoslynCompilation CommonAddSyntaxTrees(IEnumerable<RoslynSyntaxTree> trees)
        {
            return Wrap(roslynCompilation.AddSyntaxTrees(trees.Select(Unwrap)));
        }

        protected override RoslynCompilation CommonRemoveSyntaxTrees(IEnumerable<RoslynSyntaxTree> trees)
        {
            return Wrap(roslynCompilation.RemoveSyntaxTrees(trees.Select(Unwrap)));
        }

        protected override RoslynCompilation CommonRemoveAllSyntaxTrees()
        {
            return Wrap(roslynCompilation.RemoveAllSyntaxTrees());
        }

        protected override RoslynCompilation CommonReplaceSyntaxTree(RoslynSyntaxTree oldTree, RoslynSyntaxTree newTree)
        {
            return Wrap(roslynCompilation.ReplaceSyntaxTree(Unwrap(oldTree), Unwrap(newTree)));
        }

        protected override bool CommonContainsSyntaxTree(RoslynSyntaxTree syntaxTree)
        {
            return roslynCompilation.ContainsSyntaxTree(Unwrap(syntaxTree));
        }

        public override CompilationReference ToMetadataReference(ImmutableArray<string> aliases = new ImmutableArray<string>(), bool embedInteropTypes = false)
        {
            return roslynCompilation.ToMetadataReference(aliases, embedInteropTypes);
        }

        protected override RoslynCompilation CommonWithReferences(IEnumerable<MetadataReference> newReferences)
        {
            return Wrap(roslynCompilation.WithReferences(newReferences));
        }

        protected override ISymbol CommonGetAssemblyOrModuleSymbol(MetadataReference reference)
        {
            return roslynCompilation.GetAssemblyOrModuleSymbol(reference);
        }

        protected override INamespaceSymbol CommonGetCompilationNamespace(INamespaceSymbol namespaceSymbol)
        {
            return roslynCompilation.GetCompilationNamespace(namespaceSymbol);
        }

        protected override IMethodSymbol CommonGetEntryPoint(CancellationToken cancellationToken)
        {
            return roslynCompilation.GetEntryPoint(cancellationToken);
        }

        protected override INamedTypeSymbol CommonGetSpecialType(SpecialType specialType)
        {
            return roslynCompilation.GetSpecialType(specialType);
        }

        protected override IArrayTypeSymbol CommonCreateArrayTypeSymbol(ITypeSymbol elementType, int rank)
        {
            return roslynCompilation.CreateArrayTypeSymbol(elementType, rank);
        }

        protected override IPointerTypeSymbol CommonCreatePointerTypeSymbol(ITypeSymbol elementType)
        {
            return roslynCompilation.CreatePointerTypeSymbol(elementType);
        }

        protected override INamedTypeSymbol CommonGetTypeByMetadataName(string metadataName)
        {
            return roslynCompilation.GetTypeByMetadataName(metadataName);
        }

        protected override INamedTypeSymbol CommonCreateTupleTypeSymbol(
            ImmutableArray<ITypeSymbol> elementTypes, ImmutableArray<string> elementNames, ImmutableArray<Location> elementLocations)
        {
            return roslynCompilation.CreateTupleTypeSymbol(elementTypes, elementNames, elementLocations);
        }

        protected override INamedTypeSymbol CommonCreateTupleTypeSymbol(
            INamedTypeSymbol underlyingType, ImmutableArray<string> elementNames, ImmutableArray<Location> elementLocations)
        {
            return roslynCompilation.CreateTupleTypeSymbol(underlyingType, elementNames, elementLocations);
        }

        protected override INamedTypeSymbol CommonCreateAnonymousTypeSymbol(
            ImmutableArray<ITypeSymbol> memberTypes, ImmutableArray<string> memberNames, ImmutableArray<Location> memberLocations,
            ImmutableArray<bool> memberIsReadOnly)
        {
            return roslynCompilation.CreateAnonymousTypeSymbol(memberTypes, memberNames, memberIsReadOnly, memberLocations);
        }

        public override CommonConversion ClassifyCommonConversion(ITypeSymbol source, ITypeSymbol destination)
        {
            return roslynCompilation.ClassifyCommonConversion(source, destination);
        }

        public override ImmutableArray<Diagnostic> GetParseDiagnostics(CancellationToken cancellationToken = new CancellationToken())
        {
            return roslynCompilation.GetParseDiagnostics(cancellationToken);
        }

        public override ImmutableArray<Diagnostic> GetDeclarationDiagnostics(CancellationToken cancellationToken = new CancellationToken())
        {
            return roslynCompilation.GetDeclarationDiagnostics(cancellationToken);
        }

        public override ImmutableArray<Diagnostic> GetMethodBodyDiagnostics(CancellationToken cancellationToken = new CancellationToken())
        {
            return roslynCompilation.GetMethodBodyDiagnostics(cancellationToken);
        }

        public override ImmutableArray<Diagnostic> GetDiagnostics(CancellationToken cancellationToken = new CancellationToken())
        {
            return roslynCompilation.GetDiagnostics(cancellationToken);
        }

        protected override void AppendDefaultVersionResource(Stream resourceStream)
        {
            throw new NotImplementedException();
        }

        public override bool ContainsSymbolsWithName(
            Func<string, bool> predicate, SymbolFilter filter = SymbolFilter.TypeAndMember, CancellationToken cancellationToken = new CancellationToken())
        {
            return roslynCompilation.ContainsSymbolsWithName(predicate, filter, cancellationToken);
        }

        public override IEnumerable<ISymbol> GetSymbolsWithName(
            Func<string, bool> predicate, SymbolFilter filter = SymbolFilter.TypeAndMember, CancellationToken cancellationToken = new CancellationToken())
        {
            return roslynCompilation.GetSymbolsWithName(predicate, filter, cancellationToken);
        }

        public override bool ContainsSymbolsWithName(
            string name, SymbolFilter filter = SymbolFilter.TypeAndMember, CancellationToken cancellationToken = new CancellationToken())
        {
            return roslynCompilation.ContainsSymbolsWithName(name, filter, cancellationToken);
        }

        public override IEnumerable<ISymbol> GetSymbolsWithName(
            string name, SymbolFilter filter = SymbolFilter.TypeAndMember, CancellationToken cancellationToken = new CancellationToken())
        {
            return roslynCompilation.GetSymbolsWithName(name, filter, cancellationToken);
        }

        public override AnalyzerDriver AnalyzerForLanguage(ImmutableArray<DiagnosticAnalyzer> analyzers, AnalyzerManager analyzerManager)
        {
            return roslynCompilation.AnalyzerForLanguage(analyzers, analyzerManager);
        }

        public override RoslynCompilation WithEventQueue(AsyncQueue<CompilationEvent> eventQueue)
        {
            return Wrap(roslynCompilation.WithEventQueue(eventQueue));
        }

        public override bool HasSubmissionResult()
        {
            throw new NotImplementedException();
        }

        public override CommonReferenceManager CommonGetBoundReferenceManager()
        {
            throw new NotImplementedException();
        }

        public override ISymbol CommonGetSpecialTypeMember(SpecialMember specialMember)
        {
            throw new NotImplementedException();
        }

        public override bool IsSystemTypeReference(ITypeSymbol type)
        {
            throw new NotImplementedException();
        }

        public override ISymbol CommonGetWellKnownTypeMember(WellKnownMember member)
        {
            throw new NotImplementedException();
        }

        public override bool IsAttributeType(ITypeSymbol type)
        {
            throw new NotImplementedException();
        }

        public override IConvertibleConversion ClassifyConvertibleConversion(IOperation source, ITypeSymbol destination, out Optional<object> constantValue)
        {
            throw new NotImplementedException();
        }

        public override void GetDiagnostics(CompilationStage stage, bool includeEarlierStages, DiagnosticBag diagnostics, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public override bool HasCodeToEmit()
        {
            throw new NotImplementedException();
        }

        public override CommonPEModuleBuilder CreateModuleBuilder(EmitOptions emitOptions, IMethodSymbol debugEntryPoint, Stream sourceLinkStream, IEnumerable<EmbeddedText> embeddedTexts, IEnumerable<ResourceDescription> manifestResources, CompilationTestData testData, DiagnosticBag diagnostics, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public override bool CompileMethods(CommonPEModuleBuilder moduleBuilder, bool emittingPdb, bool emitMetadataOnly, bool emitTestCoverageData, DiagnosticBag diagnostics, Predicate<ISymbol> filterOpt, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public override void AddDebugSourceDocumentsForChecksumDirectives(DebugDocumentsBuilder documentsBuilder, RoslynSyntaxTree tree, DiagnosticBag diagnostics)
        {
            throw new NotImplementedException();
        }

        public override bool GenerateResourcesAndDocumentationComments(CommonPEModuleBuilder moduleBeingBuilt, Stream xmlDocumentationStream, Stream win32ResourcesStream, string outputNameOverride, DiagnosticBag diagnostics, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public override void ReportUnusedImports(RoslynSyntaxTree filterTree, DiagnosticBag diagnostics, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public override void CompleteTrees(RoslynSyntaxTree filterTree)
        {
            throw new NotImplementedException();
        }

        public override EmitDifferenceResult EmitDifference(EmitBaseline baseline, IEnumerable<SemanticEdit> edits, Func<ISymbol, bool> isAddedSymbol, Stream metadataStream, Stream ilStream, Stream pdbStream, ICollection<MethodDefinitionHandle> updatedMethodHandles, CompilationTestData testData, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public override void ValidateDebugEntryPoint(IMethodSymbol debugEntryPoint, DiagnosticBag diagnostics)
        {
            throw new NotImplementedException();
        }

        public override int GetSyntaxTreeOrdinal(RoslynSyntaxTree tree)
        {
            throw new NotImplementedException();
        }

        public override int CompareSourceLocations(Location loc1, Location loc2)
        {
            throw new NotImplementedException();
        }

        public override int CompareSourceLocations(SyntaxReference loc1, SyntaxReference loc2)
        {
            throw new NotImplementedException();
        }

        public override bool IsUnreferencedAssemblyIdentityDiagnosticCode(int code)
        {
            throw new NotImplementedException();
        }

        public override bool IsCaseSensitive => roslynCompilation.IsCaseSensitive;

        public override string Language => roslynCompilation.Language;

        protected override CompilationOptions CommonOptions => roslynCompilation.Options;

        protected override IEnumerable<RoslynSyntaxTree> CommonSyntaxTrees => roslynCompilation.SyntaxTrees;

        public override ImmutableArray<MetadataReference> DirectiveReferences => roslynCompilation.DirectiveReferences;

        public override IEnumerable<AssemblyIdentity> ReferencedAssemblyNames => roslynCompilation.ReferencedAssemblyNames;

        protected override IAssemblySymbol CommonAssembly => roslynCompilation.Assembly;

        protected override IModuleSymbol CommonSourceModule => roslynCompilation.SourceModule;

        protected override INamespaceSymbol CommonGlobalNamespace => roslynCompilation.GlobalNamespace;

        protected override INamedTypeSymbol CommonObjectType => roslynCompilation.ObjectType;

        protected override ITypeSymbol CommonDynamicType => roslynCompilation.DynamicType;

        protected override INamedTypeSymbol CommonScriptClass => roslynCompilation.ScriptClass;

        public override ScriptCompilationInfo CommonScriptCompilationInfo => throw new NotImplementedException();

        public override IEnumerable<ReferenceDirective> ReferenceDirectives => throw new NotImplementedException();

        public override IDictionary<(string path, string content), MetadataReference> ReferenceDirectiveMap => throw new NotImplementedException();

        public override CommonAnonymousTypeManager CommonAnonymousTypeManager => throw new NotImplementedException();

        public override CommonMessageProvider MessageProvider => throw new NotImplementedException();

        public override byte LinkerMajorVersion => throw new NotImplementedException();

        public override bool IsDelaySigned => throw new NotImplementedException();

        public override StrongNameKeys StrongNameKeys => throw new NotImplementedException();

        public override Guid DebugSourceDocumentLanguageId => throw new NotImplementedException();
    }
}
