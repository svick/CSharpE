using System;
using System.Collections.Generic;
using System.Linq;
using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static CSharpE.Syntax.MemberModifiers;
using RoslynSyntaxFactory = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Roslyn = Microsoft.CodeAnalysis;

namespace CSharpE.Syntax
{
    public sealed class FinalizerDefinition : BaseMethodDefinition, ISyntaxWrapper<DestructorDeclarationSyntax>
    {
        private DestructorDeclarationSyntax syntax;

        private protected override BaseMethodDeclarationSyntax BaseMethodSyntax => syntax;

        internal FinalizerDefinition(DestructorDeclarationSyntax syntax, TypeDefinition parent)
            : base(syntax)
        {
            Init(syntax);
            Parent = parent;
        }

        private void Init(DestructorDeclarationSyntax syntax)
        {
            this.syntax = syntax;

            Modifiers = FromRoslyn.MemberModifiers(syntax.Modifiers);
        }

        public FinalizerDefinition(MemberModifiers modifiers, BlockStatement statementBody)
        {
            Modifiers = modifiers;
            StatementBody = statementBody;
        }

        #region Modifiers

        private const MemberModifiers ValidMethodModifiers = Extern | Unsafe;

        private protected override void ValidateModifiers(MemberModifiers value)
        {
            var invalidModifiers = value & ~ValidMethodModifiers;
            if (invalidModifiers != 0)
                throw new ArgumentException($"The modifiers {invalidModifiers} are not valid for a finalizer.", nameof(value));
        }

        #endregion

        DestructorDeclarationSyntax ISyntaxWrapper<DestructorDeclarationSyntax>.GetWrapped(ref bool? changed)
        {
            if (parameters?.Any() == true)
                throw new InvalidOperationException("Finalizer can't have parameters.");

            GetAndResetChanged(ref changed, out var thisChanged);

            var newAttributes = attributes?.GetWrapped(ref thisChanged) ?? syntax?.AttributeLists ?? default;
            var newModifiers = Modifiers;
            var newStatementBody = statementBodySet ? statementBody?.GetWrapped(ref thisChanged) : syntax.Body;
            var newExpressionBody = expressionBodySet ? expressionBody?.GetWrapped(ref thisChanged) : syntax.ExpressionBody?.Expression;

            if (syntax == null || newModifiers != FromRoslyn.MemberModifiers(syntax.Modifiers) ||
                thisChanged == true || ShouldAnnotate(syntax, changed))
            {
                if (Parent == null)
                    throw new InvalidOperationException("Can't create syntax node for finalizer with no parent type.");

                var arrowClause = newExpressionBody == null ? null : RoslynSyntaxFactory.ArrowExpressionClause(newExpressionBody);
                var semicolonToken = newStatementBody == null ? RoslynSyntaxFactory.Token(SyntaxKind.SemicolonToken) : default;

                var newSyntax = RoslynSyntaxFactory.DestructorDeclaration(
                    newAttributes, newModifiers.GetWrapped(), RoslynSyntaxFactory.Identifier(ParentType.Name),
                    RoslynSyntaxFactory.ParameterList(), newStatementBody, arrowClause)
                    .WithSemicolonToken(semicolonToken);

                syntax = Annotate(newSyntax);

                SetChanged(ref changed);
            }

            return syntax;
        }

        private protected override BaseMethodDeclarationSyntax GetWrappedBaseMethod(ref bool? changed) =>
            this.GetWrapped<DestructorDeclarationSyntax>(ref changed);

        private protected override void SetSyntaxImpl(Roslyn::SyntaxNode newSyntax)
        {
            Init((DestructorDeclarationSyntax)newSyntax);

            SetList(ref attributes, null);
            SetList(ref parameters, null);
            Set(ref statementBody, null);
            statementBodySet = false;
            Set(ref expressionBody, null);
            expressionBodySet = false;
        }

        private protected override SyntaxNode CloneImpl() =>
            new FinalizerDefinition(Modifiers, StatementBody) { Attributes = Attributes, ExpressionBody = ExpressionBody };

        public override IEnumerable<SyntaxNode> GetChildren() => Attributes.Concat(new SyntaxNode[] { StatementBody, ExpressionBody });
    }
}