extern alias msca;
using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
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
    internal class SemanticModel : CSharpSemanticModel
    {
        private readonly Compilation compilation;
        private readonly RoslynSyntaxTree oldTree;
        private readonly RoslynSyntaxTree newTree;
        private readonly CSharpSemanticModel roslynModel;

        public SemanticModel(Compilation compilation, RoslynSyntaxTree oldTree, RoslynSyntaxTree newTree, CSharpSemanticModel roslynModel)
        {
            this.compilation = compilation;
            this.oldTree = oldTree;
            this.newTree = newTree;
            this.roslynModel = roslynModel;
        }

        private SyntaxTreeDiff GetTreeDiff() => compilation.Diff.ForTree(roslynModel.SyntaxTree.FilePath);

        private SyntaxTreeDiff GetReverseTreeDiff() => compilation.Diff.ForTreeReverse(roslynModel.SyntaxTree.FilePath);

        private TNode Adjust<TNode>(TNode node) where TNode : SyntaxNode => GetTreeDiff().Adjust(node);

        protected override IOperation GetOperationCore(SyntaxNode node, CancellationToken cancellationToken)
        {
            return roslynModel.GetOperation(node, cancellationToken);
        }

        public override ImmutableArray<Diagnostic> GetSyntaxDiagnostics(
            TextSpan? span = null, CancellationToken cancellationToken = new CancellationToken())
        {
            return roslynModel.GetSyntaxDiagnostics(span, cancellationToken);
        }

        public override ImmutableArray<Diagnostic> GetDeclarationDiagnostics(
            TextSpan? span = null, CancellationToken cancellationToken = new CancellationToken())
        {
            var diagnostics = roslynModel.GetDeclarationDiagnostics(span == null ? null : GetTreeDiff().Adjust(span.Value), cancellationToken);

            if (!diagnostics.Any())
                return diagnostics;

            return Roslyn::ImmutableArrayExtensions.SelectAsArray(diagnostics, d => GetReverseTreeDiff().Adjust(d));
        }

        public override ImmutableArray<Diagnostic> GetMethodBodyDiagnostics(
            TextSpan? span = null, CancellationToken cancellationToken = new CancellationToken())
        {
            return roslynModel.GetMethodBodyDiagnostics(span, cancellationToken);
        }

        public override ImmutableArray<Diagnostic> GetDiagnostics(TextSpan? span = null, CancellationToken cancellationToken = new CancellationToken())
        {
            return roslynModel.GetDiagnostics(span, cancellationToken);
        }

        public override void ComputeDeclarationsInSpan(TextSpan span, bool getSymbol, Roslyn::PooledObjects.ArrayBuilder<DeclarationInfo> builder, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public override void ComputeDeclarationsInNode(SyntaxNode node, bool getSymbol, Roslyn::PooledObjects.ArrayBuilder<DeclarationInfo> builder, CancellationToken cancellationToken, int? levelsToCompute = null)
        {
            roslynModel.ComputeDeclarationsInNode(Adjust(node), getSymbol, builder, cancellationToken, levelsToCompute);
        }

        public override SymbolInfo GetSymbolInfoWorker(CSharpSyntaxNode node, SymbolInfoOptions options, CancellationToken cancellationToken = default)
            => roslynModel.GetSymbolInfoWorker(Adjust(node), options, cancellationToken);

        public override SymbolInfo GetCollectionInitializerSymbolInfoWorker(InitializerExpressionSyntax collectionInitializer, ExpressionSyntax node, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public override CSharpTypeInfo GetTypeInfoWorker(CSharpSyntaxNode node, CancellationToken cancellationToken = default)
            => roslynModel.GetTypeInfoWorker(Adjust(node), cancellationToken);

        public override ImmutableArray<Symbol> GetMemberGroupWorker(CSharpSyntaxNode node, SymbolInfoOptions options, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public override ImmutableArray<PropertySymbol> GetIndexerGroupWorker(CSharpSyntaxNode node, SymbolInfoOptions options, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public override Optional<object> GetConstantValueWorker(CSharpSyntaxNode node, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

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
            => roslynModel.GetEnclosingBinderInternal(GetTreeDiff().Adjust(position) ?? throw new NotImplementedException());

        public override MemberSemanticModel GetMemberModel(SyntaxNode node)
        {
            throw new NotImplementedException();
        }

        public override bool TryGetSpeculativeSemanticModelForMethodBodyCore(SyntaxTreeSemanticModel parentModel, int position, BaseMethodDeclarationSyntax method, out RoslynSemanticModel speculativeModel)
        {
            throw new NotImplementedException();
        }

        public override bool TryGetSpeculativeSemanticModelForMethodBodyCore(SyntaxTreeSemanticModel parentModel, int position, AccessorDeclarationSyntax accessor, out RoslynSemanticModel speculativeModel)
        {
            throw new NotImplementedException();
        }

        public override bool TryGetSpeculativeSemanticModelCore(SyntaxTreeSemanticModel parentModel, int position, TypeSyntax type, SpeculativeBindingOption bindingOption, out RoslynSemanticModel speculativeModel)
        {
            throw new NotImplementedException();
        }

        public override bool TryGetSpeculativeSemanticModelCore(SyntaxTreeSemanticModel parentModel, int position, StatementSyntax statement, out RoslynSemanticModel speculativeModel)
        {
            throw new NotImplementedException();
        }

        public override bool TryGetSpeculativeSemanticModelCore(SyntaxTreeSemanticModel parentModel, int position, EqualsValueClauseSyntax initializer, out RoslynSemanticModel speculativeModel)
        {
            throw new NotImplementedException();
        }

        public override bool TryGetSpeculativeSemanticModelCore(SyntaxTreeSemanticModel parentModel, int position, ArrowExpressionClauseSyntax expressionBody, out RoslynSemanticModel speculativeModel)
        {
            throw new NotImplementedException();
        }

        public override bool TryGetSpeculativeSemanticModelCore(SyntaxTreeSemanticModel parentModel, int position, ConstructorInitializerSyntax constructorInitializer, out RoslynSemanticModel speculativeModel)
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
            => roslynModel.GetDeclaredSymbol(Adjust(declarationSyntax), cancellationToken);

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
            => roslynModel.GetDeclaredSymbol(Adjust(declarationSyntax), cancellationToken);

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
            => roslynModel.GetDeclaredSymbols(Adjust(declarationSyntax), cancellationToken);

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

        public override CSharpCompilation Compilation => compilation.RoslynCompilation;

        public override CSharpSyntaxNode Root => (CSharpSyntaxNode)newTree.GetRoot();

        public override CSharpSemanticModel ParentModel => throw new NotImplementedException();

        public override RoslynSyntaxTree SyntaxTree => oldTree;
    }
}
