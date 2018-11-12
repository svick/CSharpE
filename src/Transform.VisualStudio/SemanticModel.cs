extern alias msca;
using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using RoslynSemanticModel = Microsoft.CodeAnalysis.SemanticModel;
using RoslynSyntaxTree = Microsoft.CodeAnalysis.SyntaxTree;
using Roslyn = msca.Microsoft.CodeAnalysis;

namespace CSharpE.Transform.VisualStudio
{
    internal class SemanticModel : SyntaxTreeSemanticModel
    {
        private readonly Compilation compilation;
        private readonly RoslynSyntaxTree oldTree;
        private readonly RoslynSyntaxTree newTree;
        private readonly CSharpSemanticModel roslynModel;

        public SemanticModel(Compilation compilation, RoslynSyntaxTree oldTree, RoslynSyntaxTree newTree, CSharpSemanticModel roslynModel)
            : base(compilation.DesignTimeCompilation, newTree)
        {
            this.compilation = compilation ?? throw new ArgumentNullException(nameof(compilation));
            this.oldTree = oldTree;
            this.newTree = newTree;
            this.roslynModel = roslynModel;
        }

        private SyntaxTreeDiff GetTreeDiff() => compilation.Diff.ForTree(roslynModel.SyntaxTree.FilePath);

        private SyntaxTreeDiff GetReverseTreeDiff() => compilation.Diff.ForTreeReverse(roslynModel.SyntaxTree.FilePath);

        private TNode Adjust<TNode>(TNode node) where TNode : SyntaxNode
        {
            if (node is CompilationUnitSyntax)
                return (TNode)newTree.GetRoot();

            return newTree.GetRoot().GetAnnotatedNodes(Annotation.Get(node)).Cast<TNode>().FirstOrDefault();
        }

        protected override IOperation GetOperationCore(SyntaxNode node, CancellationToken cancellationToken)
        {
            return roslynModel.GetOperation(node, cancellationToken);
        }

        private ImmutableArray<Diagnostic> GetDiagnosticsTemplate(
            Func<TextSpan?, CancellationToken, ImmutableArray<Diagnostic>> templateFunction,
            TextSpan? span, CancellationToken cancellationToken)
        {
            var diagnostics = templateFunction(span == null ? null : GetTreeDiff().Adjust(span.Value), cancellationToken);

            // PERF: cache result of GetReverseTreeDiff()
            return ImmutableArray.CreateRange(diagnostics, d => GetReverseTreeDiff().Adjust(d));
        }

        public override ImmutableArray<Diagnostic> GetSyntaxDiagnostics(
            TextSpan? span = null, CancellationToken cancellationToken = default)
            => GetDiagnosticsTemplate(roslynModel.GetSyntaxDiagnostics, span, cancellationToken);

        public override ImmutableArray<Diagnostic> GetDeclarationDiagnostics(
            TextSpan? span = null, CancellationToken cancellationToken = default)
            => GetDiagnosticsTemplate(roslynModel.GetDeclarationDiagnostics, span, cancellationToken);

        public override ImmutableArray<Diagnostic> GetMethodBodyDiagnostics(
            TextSpan? span = null, CancellationToken cancellationToken = default)
            => GetDiagnosticsTemplate(roslynModel.GetMethodBodyDiagnostics, span, cancellationToken);

        public override ImmutableArray<Diagnostic> GetDiagnostics(TextSpan? span = null, CancellationToken cancellationToken = default)
        {
            // HACK: Roslyn's CompilationWithAnalyzers.GenerateCompilationEventsAndPopulateEventsCacheAsync
            // assumes that after this method returns, the compilation's EventQueue will not be empty.
            // The following code should ensure that at least one event propagated from DesignTimeCompilation to the main compilation.

            ImmutableArray<Diagnostic> Core() => GetDiagnosticsTemplate(roslynModel.GetDiagnostics, span, cancellationToken);

            // In these cases, waiting for events is not necessary.
            if (span != null || compilation.EventQueue == null)
                return Core();

            bool firstTime;

            lock (compilation.eventQueueProcessingLock)
            {
                firstTime = !compilation.completedCompilationUnits.Contains(newTree);

                if (firstTime)
                    compilation.eventQueueProcessingSemaphoreCounter++;
            }

            if (!firstTime)
                return Core();

            bool finishedWaiting = false;

            try
            {
                var result = Core();

                compilation.eventQueueProcessingSemaphore.Wait();

                finishedWaiting = true;

                return result;
            }
            finally
            {
                if (!finishedWaiting)
                {
                    bool decremented = false;

                    // PERF: Use CompleteExchange instead of a lock
                    lock (compilation.eventQueueProcessingLock)
                    {
                        if (compilation.eventQueueProcessingSemaphoreCounter > 0)
                        {
                            compilation.eventQueueProcessingSemaphoreCounter--;
                            decremented = true;
                        }
                    }

                    // somebody decremented the counter to zero, so there should be a free semaphore Wait
                    if (!decremented)
                        compilation.eventQueueProcessingSemaphore.Wait();
                }
            }
        }

        public override void ComputeDeclarationsInSpan(TextSpan span, bool getSymbol, Roslyn::PooledObjects.ArrayBuilder<DeclarationInfo> builder, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public override void ComputeDeclarationsInNode(SyntaxNode node, bool getSymbol, Roslyn::PooledObjects.ArrayBuilder<DeclarationInfo> builder, CancellationToken cancellationToken, int? levelsToCompute = null)
        {
            roslynModel.ComputeDeclarationsInNode(Adjust(node), getSymbol, builder, cancellationToken, levelsToCompute);
        }

        private TInfo GetInfo<TInfo>(CSharpSyntaxNode node, Func<CSharpSyntaxNode, TInfo> getInfo, Func<int, CSharpSyntaxNode, TInfo> getSpeculativeInfo)
        {
            var adjustedNode = Adjust(node);

            if (adjustedNode != null)
                return getInfo(adjustedNode);

            int adjustedPosition = GetTreeDiff().AdjustLoose(node.Position);

            return getSpeculativeInfo(adjustedPosition, node);
        }

        public override SymbolInfo GetSymbolInfoWorker(CSharpSyntaxNode node, SymbolInfoOptions options, CancellationToken cancellationToken = default)
            => GetInfo(node, n => roslynModel.GetSymbolInfoWorker(n, options, cancellationToken),
                (p, n) => roslynModel.GetSpeculativeSymbolInfo(p, n, SpeculativeBindingOption.BindAsExpression));

        public override SymbolInfo GetCollectionInitializerSymbolInfoWorker(InitializerExpressionSyntax collectionInitializer, ExpressionSyntax node, CancellationToken cancellationToken = default)
            => roslynModel.GetCollectionInitializerSymbolInfoWorker(Adjust(collectionInitializer), Adjust(node), cancellationToken);

        public override CSharpTypeInfo GetTypeInfoWorker(CSharpSyntaxNode node, CancellationToken cancellationToken = default)
            => GetInfo(node, n => roslynModel.GetTypeInfoWorker(n, cancellationToken),
                (p, n) => roslynModel.GetSpeculativeTypeInfoWorker(
                    p, n as ExpressionSyntax ?? throw new NotImplementedException(), SpeculativeBindingOption.BindAsExpression));

        public override ImmutableArray<Symbol> GetMemberGroupWorker(CSharpSyntaxNode node, SymbolInfoOptions options, CancellationToken cancellationToken = default)
        {
            var adjusted = Adjust(node);

            if (adjusted == null)
                return ImmutableArray<Symbol>.Empty;

            return roslynModel.GetMemberGroupWorker(adjusted, options, cancellationToken);
        }

        public override ImmutableArray<PropertySymbol> GetIndexerGroupWorker(CSharpSyntaxNode node, SymbolInfoOptions options, CancellationToken cancellationToken = default)
            => roslynModel.GetIndexerGroupWorker(Adjust(node), options, cancellationToken);

        public override Optional<object> GetConstantValueWorker(CSharpSyntaxNode node, CancellationToken cancellationToken = default) =>
            roslynModel.GetConstantValueWorker(Adjust(node), cancellationToken);

        public override SymbolInfo GetSymbolInfo(OrderingSyntax node, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public override SymbolInfo GetSymbolInfo(SelectOrGroupClauseSyntax node, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public override TypeInfo GetTypeInfo(SelectOrGroupClauseSyntax node, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public override QueryClauseInfo GetQueryClauseInfo(QueryClauseSyntax node, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public override Binder GetEnclosingBinderInternal(int position)
        {
            int? adjusted = GetTreeDiff().Adjust(position);

            if (adjusted == null)
                return null;

            return roslynModel.GetEnclosingBinderInternal(adjusted.Value);
        }

        public override MemberSemanticModel GetMemberModel(SyntaxNode node) => roslynModel.GetMemberModel(Adjust(node));

        public override bool TryGetSpeculativeSemanticModelCore(SyntaxTreeSemanticModel parentModel, int position, ArrowExpressionClauseSyntax expressionBody, out RoslynSemanticModel speculativeModel)
        {
            throw new NotImplementedException();
        }

        public override bool TryGetSpeculativeSemanticModelCore(SyntaxTreeSemanticModel parentModel, int position, CrefSyntax crefSyntax, out RoslynSemanticModel speculativeModel)
        {
            throw new NotImplementedException();
        }

        public override Conversion ClassifyConversion(ExpressionSyntax expression, ITypeSymbol destination, bool isExplicitInSource = false)
        {
            throw new NotImplementedException();
        }

        public override Conversion ClassifyConversionForCast(ExpressionSyntax expression, TypeSymbol destination)
        {
            throw new NotImplementedException();
        }

        public override ISymbol GetDeclaredSymbol(MemberDeclarationSyntax declarationSyntax, CancellationToken cancellationToken = default)
        {
            var adjusted = Adjust(declarationSyntax);

            if (adjusted == null)
                return null;

            return roslynModel.GetDeclaredSymbol(adjusted, cancellationToken);
        }

        public override ISymbol GetDeclaredSymbol(LocalFunctionStatementSyntax declarationSyntax, CancellationToken cancellationToken = default)
            => roslynModel.GetDeclaredSymbol(Adjust(declarationSyntax), cancellationToken);

        public override INamespaceSymbol GetDeclaredSymbol(NamespaceDeclarationSyntax declarationSyntax, CancellationToken cancellationToken = default)
            => roslynModel.GetDeclaredSymbol(Adjust(declarationSyntax), cancellationToken);

        public override INamedTypeSymbol GetDeclaredSymbol(BaseTypeDeclarationSyntax declarationSyntax, CancellationToken cancellationToken = default)
            => roslynModel.GetDeclaredSymbol(Adjust(declarationSyntax), cancellationToken);

        public override INamedTypeSymbol GetDeclaredSymbol(DelegateDeclarationSyntax declarationSyntax, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public override IFieldSymbol GetDeclaredSymbol(EnumMemberDeclarationSyntax declarationSyntax, CancellationToken cancellationToken = default)
            => roslynModel.GetDeclaredSymbol(Adjust(declarationSyntax), cancellationToken);

        public override IMethodSymbol GetDeclaredSymbol(BaseMethodDeclarationSyntax declarationSyntax, CancellationToken cancellationToken = default)
            => roslynModel.GetDeclaredSymbol(Adjust(declarationSyntax), cancellationToken);

        public override ISymbol GetDeclaredSymbol(BasePropertyDeclarationSyntax declarationSyntax, CancellationToken cancellationToken = default)
            => roslynModel.GetDeclaredSymbol(Adjust(declarationSyntax), cancellationToken);

        public override IPropertySymbol GetDeclaredSymbol(PropertyDeclarationSyntax declarationSyntax, CancellationToken cancellationToken = default)
            => roslynModel.GetDeclaredSymbol(Adjust(declarationSyntax), cancellationToken);

        public override IPropertySymbol GetDeclaredSymbol(IndexerDeclarationSyntax declarationSyntax, CancellationToken cancellationToken = default)
            => roslynModel.GetDeclaredSymbol(Adjust(declarationSyntax), cancellationToken);

        public override IEventSymbol GetDeclaredSymbol(EventDeclarationSyntax declarationSyntax, CancellationToken cancellationToken = default)
            => roslynModel.GetDeclaredSymbol(Adjust(declarationSyntax), cancellationToken);

        public override IPropertySymbol GetDeclaredSymbol(AnonymousObjectMemberDeclaratorSyntax declaratorSyntax, CancellationToken cancellationToken = default)
            => roslynModel.GetDeclaredSymbol(Adjust(declaratorSyntax), cancellationToken);

        public override INamedTypeSymbol GetDeclaredSymbol(AnonymousObjectCreationExpressionSyntax declaratorSyntax, CancellationToken cancellationToken = default)
            => roslynModel.GetDeclaredSymbol(Adjust(declaratorSyntax), cancellationToken);

        public override INamedTypeSymbol GetDeclaredSymbol(TupleExpressionSyntax declaratorSyntax, CancellationToken cancellationToken = default)
            => roslynModel.GetDeclaredSymbol(Adjust(declaratorSyntax), cancellationToken);

        public override ISymbol GetDeclaredSymbol(ArgumentSyntax declaratorSyntax, CancellationToken cancellationToken = default)
            => roslynModel.GetDeclaredSymbol(Adjust(declaratorSyntax), cancellationToken);

        public override IMethodSymbol GetDeclaredSymbol(AccessorDeclarationSyntax declarationSyntax, CancellationToken cancellationToken = default)
            => roslynModel.GetDeclaredSymbol(Adjust(declarationSyntax), cancellationToken);

        public override IMethodSymbol GetDeclaredSymbol(ArrowExpressionClauseSyntax declarationSyntax, CancellationToken cancellationToken = default)
            => roslynModel.GetDeclaredSymbol(Adjust(declarationSyntax), cancellationToken);

        public override ISymbol GetDeclaredSymbol(VariableDeclaratorSyntax declarationSyntax, CancellationToken cancellationToken = default)
        {
            var adjusted = Adjust(declarationSyntax);

            if (adjusted == null)
                return null;

            return roslynModel.GetDeclaredSymbol(adjusted, cancellationToken);
        }

        public override ISymbol GetDeclaredSymbol(SingleVariableDesignationSyntax declarationSyntax, CancellationToken cancellationToken = default)
            => roslynModel.GetDeclaredSymbol(Adjust(declarationSyntax), cancellationToken);

        public override ILabelSymbol GetDeclaredSymbol(LabeledStatementSyntax declarationSyntax, CancellationToken cancellationToken = default)
            => roslynModel.GetDeclaredSymbol(Adjust(declarationSyntax), cancellationToken);

        public override ILabelSymbol GetDeclaredSymbol(SwitchLabelSyntax declarationSyntax, CancellationToken cancellationToken = default)
            => roslynModel.GetDeclaredSymbol(Adjust(declarationSyntax), cancellationToken);

        public override IAliasSymbol GetDeclaredSymbol(UsingDirectiveSyntax declarationSyntax, CancellationToken cancellationToken = default)
            => roslynModel.GetDeclaredSymbol(Adjust(declarationSyntax), cancellationToken);

        public override IAliasSymbol GetDeclaredSymbol(ExternAliasDirectiveSyntax declarationSyntax, CancellationToken cancellationToken = default)
            => roslynModel.GetDeclaredSymbol(Adjust(declarationSyntax), cancellationToken);

        public override IParameterSymbol GetDeclaredSymbol(ParameterSyntax declarationSyntax, CancellationToken cancellationToken = default)
            => roslynModel.GetDeclaredSymbol(Adjust(declarationSyntax), cancellationToken);

        public override ImmutableArray<ISymbol> GetDeclaredSymbols(BaseFieldDeclarationSyntax declarationSyntax, CancellationToken cancellationToken = default)
        {
            var adjusted = Adjust(declarationSyntax);

            if (adjusted == null)
                return ImmutableArray<ISymbol>.Empty;

            return roslynModel.GetDeclaredSymbols(adjusted, cancellationToken);
        }

        public override ITypeParameterSymbol GetDeclaredSymbol(TypeParameterSyntax typeParameter, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public override IRangeVariableSymbol GetDeclaredSymbol(QueryClauseSyntax queryClause, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public override IRangeVariableSymbol GetDeclaredSymbol(JoinIntoClauseSyntax node, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public override IRangeVariableSymbol GetDeclaredSymbol(QueryContinuationSyntax node, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public override ForEachStatementInfo GetForEachStatementInfo(ForEachStatementSyntax node)
        {
            throw new NotImplementedException();
        }

        public override ForEachStatementInfo GetForEachStatementInfo(CommonForEachStatementSyntax node)
        {
            throw new NotImplementedException();
        }

        public override DeconstructionInfo GetDeconstructionInfo(AssignmentExpressionSyntax node)
        {
            throw new NotImplementedException();
        }

        public override DeconstructionInfo GetDeconstructionInfo(ForEachVariableStatementSyntax node)
        {
            throw new NotImplementedException();
        }

        public override AwaitExpressionInfo GetAwaitExpressionInfo(AwaitExpressionSyntax node)
        {
            throw new NotImplementedException();
        }

        public override bool IsSpeculativeSemanticModel => roslynModel.IsSpeculativeSemanticModel;

        public override int OriginalPositionForSpeculation => roslynModel.OriginalPositionForSpeculation;

        public override RoslynSemanticModel ContainingModelOrSelf => throw new NotImplementedException();

        public override CSharpCompilation Compilation
        {
            get
            {
                // Compilation is accessed from the base constructor, before compilation is assigned
                // delegate to base version of that property in that case
                if (compilation == null)
                    return base.Compilation;

                return compilation.RoslynCompilation;
            }
        }

        public override CSharpSyntaxNode Root => (CSharpSyntaxNode)oldTree.GetRoot();

        public override CSharpSemanticModel ParentModel => null;

        public override RoslynSyntaxTree SyntaxTree
        {
            get
            {
                // the same situation as Compilation
                if (oldTree == null)
                    return base.SyntaxTree;

                return oldTree;
            }
        }
    }
}
