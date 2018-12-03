using System;
using System.Collections.Generic;
using System.Linq;
using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static CSharpE.Syntax.MemberModifiers;
using CSharpSyntaxFactory = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Roslyn = Microsoft.CodeAnalysis;

namespace CSharpE.Syntax
{
    public sealed class FinalizerDefinition : BaseMethodDefinition, ISyntaxWrapper<DestructorDeclarationSyntax>
    {
        private DestructorDeclarationSyntax syntax;

        private protected override BaseMethodDeclarationSyntax BaseMethodSyntax => syntax;

        internal FinalizerDefinition(DestructorDeclarationSyntax syntax, TypeDefinition parent)
        {
            Init(syntax);
            Parent = parent;
        }

        private void Init(DestructorDeclarationSyntax syntax)
        {
            this.syntax = syntax;

            Modifiers = FromRoslyn.MemberModifiers(syntax.Modifiers);
        }

        public FinalizerDefinition(MemberModifiers modifiers, IEnumerable<Statement> body)
        {
            Modifiers = modifiers;
            Body = new BlockStatement(body?.ToList());
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
            GetAndResetChanged(ref changed);

            bool? thisChanged = false;

            var newModifiers = Modifiers;
            var newBody = bodySet ? body?.GetWrapped(ref thisChanged) : syntax.Body;

            if (syntax == null || AttributesChanged() || newModifiers != FromRoslyn.MemberModifiers(syntax.Modifiers) ||
                thisChanged == true || !IsAnnotated(syntax))
            {
                if (Parent == null)
                    throw new InvalidOperationException("Can't create syntax node for finalizer with no parent type.");

                var newSyntax = CSharpSyntaxFactory.DestructorDeclaration(
                    GetNewAttributes(), newModifiers.GetWrapped(), CSharpSyntaxFactory.Identifier(ParentType.Name),
                    CSharpSyntaxFactory.ParameterList(), newBody);

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
            ResetAttributes();

            parameters = null;
            body = null;
        }

        internal override SyntaxNode Clone()
        {
            throw new NotImplementedException();
        }
    }
}