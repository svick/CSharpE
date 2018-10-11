extern alias msca;
using System;
using System.Collections.Immutable;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using RoslynSemanticModel = Microsoft.CodeAnalysis.SemanticModel;
using Roslyn = msca.Microsoft.CodeAnalysis;

namespace CSharpE.Transform.VisualStudio
{
    internal class SemanticModel : RoslynSemanticModel
    {
        private readonly RoslynSemanticModel roslynModel;

        public SemanticModel(RoslynSemanticModel roslynModel) => this.roslynModel = roslynModel;
        protected override IOperation GetOperationCore(SyntaxNode node, CancellationToken cancellationToken)
        {
            return roslynModel.GetOperation(node, cancellationToken);
        }

        protected override SymbolInfo GetSymbolInfoCore(SyntaxNode node, CancellationToken cancellationToken = new CancellationToken())
        {
            return roslynModel.GetSymbolInfo(node, cancellationToken);
        }

        protected override SymbolInfo GetSpeculativeSymbolInfoCore(int position, SyntaxNode expression, SpeculativeBindingOption bindingOption)
        {
            return roslynModel.GetSpeculativeSymbolInfo(position, expression, bindingOption);
        }

        protected override TypeInfo GetSpeculativeTypeInfoCore(int position, SyntaxNode expression, SpeculativeBindingOption bindingOption)
        {
            return roslynModel.GetSpeculativeTypeInfo(position, expression, bindingOption);
        }

        protected override TypeInfo GetTypeInfoCore(SyntaxNode node, CancellationToken cancellationToken = new CancellationToken())
        {
            return roslynModel.GetTypeInfo(node, cancellationToken);
        }

        protected override IAliasSymbol GetAliasInfoCore(SyntaxNode nameSyntax, CancellationToken cancellationToken = new CancellationToken())
        {
            return roslynModel.GetAliasInfo(nameSyntax, cancellationToken);
        }

        protected override IAliasSymbol GetSpeculativeAliasInfoCore(int position, SyntaxNode nameSyntax, SpeculativeBindingOption bindingOption)
        {
            return roslynModel.GetSpeculativeAliasInfo(position, nameSyntax, bindingOption);
        }

        public override ImmutableArray<Diagnostic> GetSyntaxDiagnostics(
            TextSpan? span = null, CancellationToken cancellationToken = new CancellationToken())
        {
            return roslynModel.GetSyntaxDiagnostics(span, cancellationToken);
        }

        public override ImmutableArray<Diagnostic> GetDeclarationDiagnostics(
            TextSpan? span = null, CancellationToken cancellationToken = new CancellationToken())
        {
            return roslynModel.GetDeclarationDiagnostics(span, cancellationToken);
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

        protected override ISymbol GetDeclaredSymbolCore(SyntaxNode declaration, CancellationToken cancellationToken = new CancellationToken())
        {
            return roslynModel.GetDeclaredSymbol(declaration, cancellationToken);
        }

        protected override ImmutableArray<ISymbol> GetDeclaredSymbolsCore(
            SyntaxNode declaration, CancellationToken cancellationToken = new CancellationToken())
        {
            return roslynModel.GetDeclaredSymbolsForNode(declaration, cancellationToken);
        }

        protected override ImmutableArray<ISymbol> LookupSymbolsCore(
            int position, INamespaceOrTypeSymbol container, string name, bool includeReducedExtensionMethods)
        {
            return roslynModel.LookupSymbols(position, container, name, includeReducedExtensionMethods);
        }

        protected override ImmutableArray<ISymbol> LookupBaseMembersCore(int position, string name)
        {
            return roslynModel.LookupBaseMembers(position, name);
        }

        protected override ImmutableArray<ISymbol> LookupStaticMembersCore(int position, INamespaceOrTypeSymbol container, string name)
        {
            return roslynModel.LookupStaticMembers(position, container, name);
        }

        protected override ImmutableArray<ISymbol> LookupNamespacesAndTypesCore(int position, INamespaceOrTypeSymbol container, string name)
        {
            return roslynModel.LookupNamespacesAndTypes(position, container, name);
        }

        protected override ImmutableArray<ISymbol> LookupLabelsCore(int position, string name)
        {
            return roslynModel.LookupLabels(position, name);
        }

        protected override ControlFlowAnalysis AnalyzeControlFlowCore(SyntaxNode firstStatement, SyntaxNode lastStatement)
        {
            return roslynModel.AnalyzeControlFlow(firstStatement, lastStatement);
        }

        protected override ControlFlowAnalysis AnalyzeControlFlowCore(SyntaxNode statement)
        {
            return roslynModel.AnalyzeControlFlow(statement);
        }

        protected override DataFlowAnalysis AnalyzeDataFlowCore(SyntaxNode firstStatement, SyntaxNode lastStatement)
        {
            return roslynModel.AnalyzeDataFlow(firstStatement, lastStatement);
        }

        protected override DataFlowAnalysis AnalyzeDataFlowCore(SyntaxNode statementOrExpression)
        {
            return roslynModel.AnalyzeDataFlow(statementOrExpression);
        }

        protected override Optional<object> GetConstantValueCore(SyntaxNode node, CancellationToken cancellationToken = new CancellationToken())
        {
            return roslynModel.GetConstantValue(node, cancellationToken);
        }

        protected override ImmutableArray<ISymbol> GetMemberGroupCore(SyntaxNode node, CancellationToken cancellationToken = new CancellationToken())
        {
            return roslynModel.GetMemberGroup(node, cancellationToken);
        }

        protected override ISymbol GetEnclosingSymbolCore(int position, CancellationToken cancellationToken = new CancellationToken())
        {
            return roslynModel.GetEnclosingSymbol(position, cancellationToken);
        }

        protected override bool IsAccessibleCore(int position, ISymbol symbol)
        {
            return roslynModel.IsAccessible(position, symbol);
        }

        protected override bool IsEventUsableAsFieldCore(int position, IEventSymbol eventSymbol)
        {
            return roslynModel.IsEventUsableAsField(position, eventSymbol);
        }

        protected override PreprocessingSymbolInfo GetPreprocessingSymbolInfoCore(SyntaxNode nameSyntax)
        {
            return roslynModel.GetPreprocessingSymbolInfo(nameSyntax);
        }

        public override void ComputeDeclarationsInSpan(TextSpan span, bool getSymbol, Roslyn::PooledObjects.ArrayBuilder<DeclarationInfo> builder, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public override void ComputeDeclarationsInNode(SyntaxNode node, bool getSymbol, Roslyn::PooledObjects.ArrayBuilder<DeclarationInfo> builder, CancellationToken cancellationToken, int? levelsToCompute = null)
        {
            roslynModel.ComputeDeclarationsInNode(node, getSymbol, builder, cancellationToken, levelsToCompute);
        }

        public override string Language => roslynModel.Language;

        protected override Microsoft.CodeAnalysis.Compilation CompilationCore => roslynModel.Compilation;

        protected override Microsoft.CodeAnalysis.SyntaxTree SyntaxTreeCore => roslynModel.SyntaxTree;

        public override bool IsSpeculativeSemanticModel => roslynModel.IsSpeculativeSemanticModel;

        public override int OriginalPositionForSpeculation => roslynModel.OriginalPositionForSpeculation;

        protected override RoslynSemanticModel ParentModelCore => roslynModel.ParentModel;

        protected override SyntaxNode RootCore => roslynModel.Root;

        public override RoslynSemanticModel ContainingModelOrSelf => throw new NotImplementedException();
    }
}
