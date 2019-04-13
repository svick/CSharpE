using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Reflection;
using System.Reflection.Metadata;
using CSharpE.Transform.Execution;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeGen;
using Microsoft.CodeAnalysis.CSharp;
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
    // TODO: Ensure thread-safety
    internal sealed class Compilation : RoslynCompilation
    {
        public CSharpCompilation RoslynCompilation { get; }

        public Compilation(CSharpCompilation roslynCompilation, AsyncQueue<CompilationEvent> eventQueue = null)
            : this(roslynCompilation, previousCompilation: null, eventQueue) { }

        private Compilation(
            CSharpCompilation roslynCompilation, Compilation previousCompilation, AsyncQueue<CompilationEvent> eventQueue = null)
            : base(
                roslynCompilation.AssemblyName, roslynCompilation.References.ToImmutableArray(),
                new Dictionary<string, string>() /* TODO? */, roslynCompilation.IsSubmission, eventQueue)
        {
            RoslynCompilation = roslynCompilation;

            designTimeCompilation = new Lazy<CSharpCompilation>(CreateDesignTimeCompilation);

            if (previousCompilation?.hasTransformer == true && previousCompilation.References.SequenceEqual(References))
            {
                transformer = previousCompilation.transformer;
                hasTransformer = true;
            }
        }

        private Compilation Wrap(RoslynCompilation roslynCompilation) => new Compilation((CSharpCompilation)roslynCompilation, this);

        private ProjectTransformer CreateTransformer()
        {
            // PERF: caching of transformations and possibly ITransformation

            var iTransformation = RoslynCompilation.GetTypeByMetadataName(typeof(ITransformation).FullName);

            if (iTransformation == null)
                return null;

            var transformations = new List<ITransformation>();

            foreach (var reference in RoslynCompilation.References)
            {
                var referenceSymbol = (IAssemblySymbol)RoslynCompilation.GetAssemblyOrModuleSymbol(reference);
                var transformationTypes = GetAllTypesVisitor.FindTypes(
                    referenceSymbol.GlobalNamespace,
                    type => type.TypeKind != TypeKind.Interface && !type.IsAbstract && type.AllInterfaces.Contains(iTransformation));

                if (!transformationTypes.Any())
                    continue;

                Assembly assembly;

                switch (reference)
                {
                    case PortableExecutableReference peReference:

                        try
                        {
                            assembly = System.Reflection.Assembly.LoadFrom(peReference.FilePath);
                        }
                        catch (Exception ex)
                        {
                            // TODO: produce error
                            return null;
                        }

                        foreach (var symbol in transformationTypes)
                        {
                            var type = assembly.GetType(symbol.GetFullMetadataName());
                            var instance = (ITransformation)Activator.CreateInstance(type);

                            transformations.Add(instance);
                        }
                        break;
                    case CSharpCompilationReference compilationReference:
                        var memoryStream = new MemoryStream();
                        var emitResult = compilationReference.Compilation.Emit(memoryStream);

                        if (!emitResult.Success)
                        {
                            // TODO: produce error?
                            return null;
                        }

                        memoryStream.Position = 0;

                        try
                        {
                            assembly = System.Reflection.Assembly.Load(memoryStream.ToArray());
                        }
                        catch (Exception ex)
                        {
                            // TODO: produce error
                            return null;
                        }
                        break;
                    default:
                        throw new InvalidOperationException();
                }
            }

            if (!transformations.Any())
                return null;

            return new ProjectTransformer(transformations, designTime: true);
        }

        // based on https://github.com/dotnet/roslyn/issues/6138#issuecomment-149216303
        private class GetAllTypesVisitor : SymbolVisitor
        {
            public static List<INamedTypeSymbol> FindTypes(INamespaceSymbol ns, Func<INamedTypeSymbol, bool> condition)
            {
                var visitor = new GetAllTypesVisitor(condition);

                ns.Accept(visitor);

                return visitor.types;
            }

            private readonly Func<INamedTypeSymbol, bool> condition;
            private readonly List<INamedTypeSymbol> types = new List<INamedTypeSymbol>();

            private GetAllTypesVisitor(Func<INamedTypeSymbol, bool> condition) => this.condition = condition;

            public override void VisitNamespace(INamespaceSymbol symbol)
            {
                foreach (var s in symbol.GetMembers())
                {
                    s.Accept(this);
                }
            }

            public override void VisitNamedType(INamedTypeSymbol symbol)
            {
                if (condition(symbol))
                    types.Add(symbol);
            }
        }

        private bool hasTransformer;
        private ProjectTransformer transformer;
        internal ProjectTransformer Transformer
        {
            get
            {
                if (!hasTransformer)
                {
                    transformer = CreateTransformer();

                    hasTransformer = true;
                }
                return transformer;
            }
        }

        internal readonly object eventQueueProcessingLock = new object();
        internal readonly HashSet<RoslynSyntaxTree> completedCompilationUnits = new HashSet<RoslynSyntaxTree>();
        internal int eventQueueProcessingSemaphoreCounter = 0;
        internal readonly SemaphoreSlim eventQueueProcessingSemaphore = new SemaphoreSlim(0);

        private AsyncQueue<CompilationEvent> Adjust(AsyncQueue<CompilationEvent> eventQueue)
        {
            if (eventQueue == null)
                return null;

            var oldQueue = eventQueue;
            var newQueue = new AsyncQueue<CompilationEvent>();

            // if this method throws an exception, there is no good way to handle it
            // so use async void, which will raise the exception on the synchronization context
            async void PropagateEvents()
            {
                try
                {
                    while (true)
                    {
                        var e = await newQueue.DequeueAsync();

                        CompilationEvent newEvent;

                        switch (e)
                        {
                            case SymbolDeclaredCompilationEvent symbolDeclared:
                                newEvent = new SymbolDeclaredCompilationEvent(this, symbolDeclared.Symbol);
                                break;
                            case CompilationUnitCompletedEvent compilationUnitCompleted:
                                newEvent = new CompilationUnitCompletedEvent(this, AdjustReverse(compilationUnitCompleted.CompilationUnit));

                                lock (eventQueueProcessingLock)
                                {
                                    completedCompilationUnits.Add(compilationUnitCompleted.CompilationUnit);
                                }

                                break;
                            case CompilationStartedEvent compilationStarted:
                                newEvent = new CompilationStartedEvent(this);
                                break;
                            default:
                                throw new NotImplementedException();
                        }

                        oldQueue.Enqueue(newEvent);

                        if (e is CompilationUnitCompletedEvent)
                        {
                            while (true)
                            {
                                lock (eventQueueProcessingLock)
                                {
                                    if (eventQueueProcessingSemaphoreCounter == 0)
                                        break;

                                    eventQueueProcessingSemaphoreCounter--;
                                }

                                eventQueueProcessingSemaphore.Release();
                            }
                        }
                    }
                }
                catch (TaskCanceledException) { }
            }

            PropagateEvents();

            return newQueue;
        }

        private CSharpCompilation CreateDesignTimeCompilation()
        {
            if (Transformer == null)
                return RoslynCompilation;

            Syntax.Project transformed;
            lock (Transformer)
            {
                transformed = Transformer.Transform(new Syntax.Project(RoslynCompilation));
            }

            return (CSharpCompilation)CSharpCompilation.Create(
                RoslynCompilation.AssemblyName, transformed.SourceFiles.Select(file => file.GetSyntaxTree()),
                transformed.References.Select(reference => reference.GetMetadataReference()),
                RoslynCompilation.Options).WithEventQueue(Adjust(EventQueue));
        }

        private readonly Lazy<CSharpCompilation> designTimeCompilation;
        internal CSharpCompilation DesignTimeCompilation => designTimeCompilation.Value;

        private CompilationDiff diff;
        internal CompilationDiff Diff
        {
            get
            {
                if (diff == null)
                    diff = new CompilationDiff(RoslynCompilation, DesignTimeCompilation);

                return diff;
            }
        }

        private RoslynSyntaxTree AdjustReverse(RoslynSyntaxTree syntaxTree) => this.GetTree(syntaxTree.FilePath);

        private RoslynSyntaxTree Adjust(RoslynSyntaxTree syntaxTree) => DesignTimeCompilation.GetTree(syntaxTree.FilePath);

        #region overrides
        protected override RoslynCompilation CommonClone()
        {
            return Wrap(RoslynCompilation.Clone());
        }

        protected override RoslynSemanticModel CommonGetSemanticModel(RoslynSyntaxTree syntaxTree, bool ignoreAccessibility)
        {
            var newTree = Adjust(syntaxTree);
            var roslynModel = (CSharpSemanticModel)DesignTimeCompilation.GetSemanticModel(newTree, ignoreAccessibility);
            return new SemanticModel(this, syntaxTree, newTree, roslynModel);
        }

        protected override INamedTypeSymbol CommonCreateErrorTypeSymbol(INamespaceOrTypeSymbol container, string name, int arity)
        {
            return DesignTimeCompilation.CreateErrorTypeSymbol(container, name, arity);
        }

        protected override INamespaceSymbol CommonCreateErrorNamespaceSymbol(INamespaceSymbol container, string name)
        {
            return DesignTimeCompilation.CreateErrorNamespaceSymbol(container, name);
        }

        protected override RoslynCompilation CommonWithAssemblyName(string outputName)
        {
            return Wrap(RoslynCompilation.WithAssemblyName(outputName));
        }

        protected override RoslynCompilation CommonWithOptions(CompilationOptions options)
        {
            return Wrap(RoslynCompilation.WithOptions(options));
        }

        protected override RoslynCompilation CommonWithScriptCompilationInfo(ScriptCompilationInfo info)
        {
            return Wrap(RoslynCompilation.WithScriptCompilationInfo(info));
        }

        protected override RoslynCompilation CommonAddSyntaxTrees(IEnumerable<RoslynSyntaxTree> trees)
        {
            return Wrap(RoslynCompilation.AddSyntaxTrees(trees.Select(Unwrap)));
        }

        protected override RoslynCompilation CommonRemoveSyntaxTrees(IEnumerable<RoslynSyntaxTree> trees)
        {
            return Wrap(RoslynCompilation.RemoveSyntaxTrees(trees.Select(Unwrap)));
        }

        protected override RoslynCompilation CommonRemoveAllSyntaxTrees()
        {
            return Wrap(RoslynCompilation.RemoveAllSyntaxTrees());
        }

        protected override RoslynCompilation CommonReplaceSyntaxTree(RoslynSyntaxTree oldTree, RoslynSyntaxTree newTree)
        {
            return Wrap(RoslynCompilation.ReplaceSyntaxTree(Unwrap(oldTree), Unwrap(newTree)));
        }

        protected override bool CommonContainsSyntaxTree(RoslynSyntaxTree syntaxTree)
            => RoslynCompilation.ContainsSyntaxTree(syntaxTree) ||
                (designTimeCompilation.IsValueCreated && DesignTimeCompilation.ContainsSyntaxTree(syntaxTree));

        public override CompilationReference ToMetadataReference(ImmutableArray<string> aliases = default, bool embedInteropTypes = false)
        {
            return RoslynCompilation.ToMetadataReference(aliases, embedInteropTypes);
        }

        protected override RoslynCompilation CommonWithReferences(IEnumerable<MetadataReference> newReferences)
        {
            return Wrap(RoslynCompilation.WithReferences(newReferences));
        }

        protected override ISymbol CommonGetAssemblyOrModuleSymbol(MetadataReference reference)
        {
            return DesignTimeCompilation.GetAssemblyOrModuleSymbol(reference);
        }

        protected override INamespaceSymbol CommonGetCompilationNamespace(INamespaceSymbol namespaceSymbol)
        {
            return DesignTimeCompilation.GetCompilationNamespace(namespaceSymbol);
        }

        protected override IMethodSymbol CommonGetEntryPoint(CancellationToken cancellationToken)
        {
            return DesignTimeCompilation.GetEntryPoint(cancellationToken);
        }

        protected override INamedTypeSymbol CommonGetSpecialType(SpecialType specialType)
        {
            return DesignTimeCompilation.GetSpecialType(specialType);
        }

        protected override IArrayTypeSymbol CommonCreateArrayTypeSymbol(ITypeSymbol elementType, int rank)
        {
            return DesignTimeCompilation.CreateArrayTypeSymbol(elementType, rank);
        }

        protected override IPointerTypeSymbol CommonCreatePointerTypeSymbol(ITypeSymbol elementType)
        {
            return DesignTimeCompilation.CreatePointerTypeSymbol(elementType);
        }

        protected override INamedTypeSymbol CommonGetTypeByMetadataName(string metadataName)
        {
            return DesignTimeCompilation.GetTypeByMetadataName(metadataName);
        }

        protected override INamedTypeSymbol CommonCreateTupleTypeSymbol(
            ImmutableArray<ITypeSymbol> elementTypes, ImmutableArray<string> elementNames, ImmutableArray<Location> elementLocations)
        {
            return DesignTimeCompilation.CreateTupleTypeSymbol(elementTypes, elementNames, elementLocations);
        }

        protected override INamedTypeSymbol CommonCreateTupleTypeSymbol(
            INamedTypeSymbol underlyingType, ImmutableArray<string> elementNames, ImmutableArray<Location> elementLocations)
        {
            return DesignTimeCompilation.CreateTupleTypeSymbol(underlyingType, elementNames, elementLocations);
        }

        protected override INamedTypeSymbol CommonCreateAnonymousTypeSymbol(
            ImmutableArray<ITypeSymbol> memberTypes, ImmutableArray<string> memberNames, ImmutableArray<Location> memberLocations,
            ImmutableArray<bool> memberIsReadOnly)
        {
            return DesignTimeCompilation.CreateAnonymousTypeSymbol(memberTypes, memberNames, memberIsReadOnly, memberLocations);
        }

        public override CommonConversion ClassifyCommonConversion(ITypeSymbol source, ITypeSymbol destination)
        {
            return DesignTimeCompilation.ClassifyCommonConversion(source, destination);
        }

        // TODO: Adjust diagnostics
        public override ImmutableArray<Diagnostic> GetParseDiagnostics(CancellationToken cancellationToken = new CancellationToken())
        {
            return DesignTimeCompilation.GetParseDiagnostics(cancellationToken);
        }

        public override ImmutableArray<Diagnostic> GetDeclarationDiagnostics(CancellationToken cancellationToken = new CancellationToken())
        {
            return DesignTimeCompilation.GetDeclarationDiagnostics(cancellationToken);
        }

        public override ImmutableArray<Diagnostic> GetMethodBodyDiagnostics(CancellationToken cancellationToken = new CancellationToken())
        {
            return DesignTimeCompilation.GetMethodBodyDiagnostics(cancellationToken);
        }

        public override ImmutableArray<Diagnostic> GetDiagnostics(CancellationToken cancellationToken = new CancellationToken())
        {
            return DesignTimeCompilation.GetDiagnostics(cancellationToken);
        }

        protected override void AppendDefaultVersionResource(Stream resourceStream)
        {
            throw new NotImplementedException();
        }

        public override bool ContainsSymbolsWithName(
            Func<string, bool> predicate, SymbolFilter filter = SymbolFilter.TypeAndMember, CancellationToken cancellationToken = new CancellationToken())
        {
            return DesignTimeCompilation.ContainsSymbolsWithName(predicate, filter, cancellationToken);
        }

        public override IEnumerable<ISymbol> GetSymbolsWithName(
            Func<string, bool> predicate, SymbolFilter filter = SymbolFilter.TypeAndMember, CancellationToken cancellationToken = new CancellationToken())
        {
            return DesignTimeCompilation.GetSymbolsWithName(predicate, filter, cancellationToken);
        }

        public override bool ContainsSymbolsWithName(
            string name, SymbolFilter filter = SymbolFilter.TypeAndMember, CancellationToken cancellationToken = new CancellationToken())
        {
            return DesignTimeCompilation.ContainsSymbolsWithName(name, filter, cancellationToken);
        }

        public override IEnumerable<ISymbol> GetSymbolsWithName(
            string name, SymbolFilter filter = SymbolFilter.TypeAndMember, CancellationToken cancellationToken = new CancellationToken())
        {
            return DesignTimeCompilation.GetSymbolsWithName(name, filter, cancellationToken);
        }

        public override AnalyzerDriver AnalyzerForLanguage(ImmutableArray<DiagnosticAnalyzer> analyzers, AnalyzerManager analyzerManager)
        {
            return DesignTimeCompilation.AnalyzerForLanguage(analyzers, analyzerManager);
        }

        public override RoslynCompilation WithEventQueue(AsyncQueue<CompilationEvent> eventQueue)
        {
            // because most methods delegate to DesignTimeCompilation, eventQueue should be set there
            // but since it might not exist yet, save the eventQueue for now
            return new Compilation(RoslynCompilation, this, eventQueue);
        }

        public override bool HasSubmissionResult()
        {
            throw new NotImplementedException();
        }

        public override CommonReferenceManager CommonGetBoundReferenceManager() => RoslynCompilation.CommonGetBoundReferenceManager();

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

        public override bool IsCaseSensitive => RoslynCompilation.IsCaseSensitive;

        public override string Language => RoslynCompilation.Language;

        protected override CompilationOptions CommonOptions => RoslynCompilation.Options;

        protected override IEnumerable<RoslynSyntaxTree> CommonSyntaxTrees => RoslynCompilation.SyntaxTrees;

        public override ImmutableArray<MetadataReference> DirectiveReferences => RoslynCompilation.DirectiveReferences;

        public override IEnumerable<AssemblyIdentity> ReferencedAssemblyNames => DesignTimeCompilation.ReferencedAssemblyNames;

        protected override IAssemblySymbol CommonAssembly => DesignTimeCompilation.Assembly;

        protected override IModuleSymbol CommonSourceModule => DesignTimeCompilation.SourceModule;

        protected override INamespaceSymbol CommonGlobalNamespace => DesignTimeCompilation.GlobalNamespace;

        protected override INamedTypeSymbol CommonObjectType => DesignTimeCompilation.ObjectType;

        protected override ITypeSymbol CommonDynamicType => DesignTimeCompilation.DynamicType;

        protected override INamedTypeSymbol CommonScriptClass => DesignTimeCompilation.ScriptClass;

        public override ScriptCompilationInfo CommonScriptCompilationInfo => DesignTimeCompilation.ScriptCompilationInfo;

        public override IEnumerable<ReferenceDirective> ReferenceDirectives => throw new NotImplementedException();

        public override IDictionary<(string path, string content), MetadataReference> ReferenceDirectiveMap => throw new NotImplementedException();

        public override CommonAnonymousTypeManager CommonAnonymousTypeManager => throw new NotImplementedException();

        public override CommonMessageProvider MessageProvider => throw new NotImplementedException();

        public override byte LinkerMajorVersion => throw new NotImplementedException();

        public override bool IsDelaySigned => throw new NotImplementedException();

        public override StrongNameKeys StrongNameKeys => throw new NotImplementedException();

        public override Guid DebugSourceDocumentLanguageId => throw new NotImplementedException();

        #endregion
    }
}
