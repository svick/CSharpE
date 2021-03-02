using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RoslynSemanticModel = Microsoft.CodeAnalysis.SemanticModel;
using CSharpMemberSemanticModel = Microsoft.CodeAnalysis.CSharp.MemberSemanticModel;

namespace CSharpE.Transform.VisualStudio
{
    class MemberSemanticModel : CSharpMemberSemanticModel
    {
        private readonly CSharpMemberSemanticModel wrappedModel;
        private readonly SemanticModel parent;

        public MemberSemanticModel(
            CSharpMemberSemanticModel wrappedModel, SemanticModel parent)
            : base(wrappedModel.Root, wrappedModel.MemberSymbol, wrappedModel.RootBinder, wrappedModel.ContainingModelOrSelf as SyntaxTreeSemanticModel,
                  (SyntaxTreeSemanticModel)wrappedModel.ParentModel, null, null, wrappedModel.OriginalPositionForSpeculation)
        {
            this.wrappedModel = wrappedModel;
            this.parent = parent;
        }

        private delegate bool TryGetSpeculativeSemanticModelCoreDelegate<T>(SyntaxTreeSemanticModel parentModel, int position, T syntax, out RoslynSemanticModel speculativeModel);

        private bool TryGetSpeculativeSemanticModelCoreTemplate<T>(
            TryGetSpeculativeSemanticModelCoreDelegate<T> templateFunction, SyntaxTreeSemanticModel parentModel, int position, T syntax, out RoslynSemanticModel speculativeModel)
        {
            var adjusted = parent.GetTreeDiff().Adjust(position);

            if (adjusted == null)
            {
                speculativeModel = null;
                return false;
            }

            return templateFunction(parentModel, adjusted.Value, syntax, out speculativeModel);
        }

        public override bool TryGetSpeculativeSemanticModelCore(
            SyntaxTreeSemanticModel parentModel, int position, StatementSyntax statement, out RoslynSemanticModel speculativeModel) =>
            TryGetSpeculativeSemanticModelCoreTemplate(wrappedModel.TryGetSpeculativeSemanticModelCore, parentModel, position, statement, out speculativeModel);

        public override bool TryGetSpeculativeSemanticModelCore(
            SyntaxTreeSemanticModel parentModel, int position, EqualsValueClauseSyntax initializer, out RoslynSemanticModel speculativeModel) =>
            TryGetSpeculativeSemanticModelCoreTemplate(wrappedModel.TryGetSpeculativeSemanticModelCore, parentModel, position, initializer, out speculativeModel);

        public override bool TryGetSpeculativeSemanticModelCore(
            SyntaxTreeSemanticModel parentModel, int position, ArrowExpressionClauseSyntax expressionBody, out RoslynSemanticModel speculativeModel) =>
            TryGetSpeculativeSemanticModelCoreTemplate(wrappedModel.TryGetSpeculativeSemanticModelCore, parentModel, position, expressionBody, out speculativeModel);

        public override bool TryGetSpeculativeSemanticModelCore(
            SyntaxTreeSemanticModel parentModel, int position, ConstructorInitializerSyntax constructorInitializer, out RoslynSemanticModel speculativeModel) =>
            TryGetSpeculativeSemanticModelCoreTemplate(wrappedModel.TryGetSpeculativeSemanticModelCore, parentModel, position, constructorInitializer, out speculativeModel);

        public override bool TryGetSpeculativeSemanticModelForMethodBodyCore(
            SyntaxTreeSemanticModel parentModel, int position, BaseMethodDeclarationSyntax method, out RoslynSemanticModel speculativeModel) =>
            TryGetSpeculativeSemanticModelCoreTemplate(wrappedModel.TryGetSpeculativeSemanticModelForMethodBodyCore, parentModel, position, method, out speculativeModel);

        public override bool TryGetSpeculativeSemanticModelForMethodBodyCore(
            SyntaxTreeSemanticModel parentModel, int position, AccessorDeclarationSyntax accessor, out RoslynSemanticModel speculativeModel) =>
            TryGetSpeculativeSemanticModelCoreTemplate(wrappedModel.TryGetSpeculativeSemanticModelForMethodBodyCore, parentModel, position, accessor, out speculativeModel);

        protected override BoundNode RewriteNullableBoundNodesWithSnapshots(
            BoundNode boundRoot, Binder binder, DiagnosticBag diagnostics, bool takeSnapshots,
            out NullableWalker.SnapshotManager snapshotManager, ref ImmutableDictionary<Symbol, Symbol> remappedSymbols)
            => throw new NotImplementedException();

        public override bool TryGetSpeculativeSemanticModelCore(SyntaxTreeSemanticModel parentModel, int position, PrimaryConstructorBaseTypeSyntax constructorInitializer, out RoslynSemanticModel speculativeModel)
            => throw new NotImplementedException();
    }
}
