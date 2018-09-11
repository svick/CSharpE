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
    public sealed class ConstructorDefinition : BaseMethodDefinition, ISyntaxWrapper<ConstructorDeclarationSyntax>
    {
        private ConstructorDeclarationSyntax syntax;

        private protected override BaseMethodDeclarationSyntax BaseMethodSyntax => syntax;

        internal ConstructorDefinition(ConstructorDeclarationSyntax syntax, TypeDefinition parent)
        {
            Init(syntax);
            Parent = parent;
        }

        private void Init(ConstructorDeclarationSyntax syntax)
        {
            this.syntax = syntax;

            Modifiers = FromRoslyn.MemberModifiers(syntax.Modifiers);
        }

        public ConstructorDefinition(
            MemberModifiers modifiers, IEnumerable<Parameter> parameters, IEnumerable<Statement> body)
        {
            Modifiers = modifiers;
            Parameters = parameters?.ToList();
            Body = new BlockStatement(body?.ToList());
        }

        #region Modifiers

        private const MemberModifiers ValidMethodModifiers =
            AccessModifiersMask | Extern | Unsafe | Static;

        private protected override void ValidateModifiers(MemberModifiers value)
        {
            var invalidModifiers = value & ~ValidMethodModifiers;
            if (invalidModifiers != 0)
                throw new ArgumentException($"The modifiers {invalidModifiers} are not valid for a constructor.", nameof(value));
        }

        public bool IsStatic
        {
            get => Modifiers.Contains(Static);
            set => Modifiers = Modifiers.With(Static, value);
        }

        #endregion

        // TODO: initializers (: base and : this)

        ConstructorDeclarationSyntax ISyntaxWrapper<ConstructorDeclarationSyntax>.GetWrapped(ref bool? changed)
        {
            GetAndResetChanged(ref changed);

            bool? thisChanged = false;

            var newModifiers = Modifiers;
            var newParameters = parameters?.GetWrapped(ref thisChanged) ?? syntax.ParameterList.Parameters;
            var newBody = body?.GetWrapped(ref thisChanged) ?? syntax.Body;

            if (syntax == null || AttributesChanged() || newModifiers != FromRoslyn.MemberModifiers(syntax.Modifiers) ||
                thisChanged == true || !IsAnnotated(syntax))
            {
                if (Parent == null)
                    throw new InvalidOperationException("Can't create syntax ndoe for constructor with no parent type.");

                var newSyntax = CSharpSyntaxFactory.ConstructorDeclaration(
                    GetNewAttributes(), newModifiers.GetWrapped(), CSharpSyntaxFactory.Identifier(ParentType.Name),
                    CSharpSyntaxFactory.ParameterList(newParameters), default, newBody);

                syntax = Annotate(newSyntax);

                SetChanged(ref changed);
            }

            return syntax;
        }

        private protected override BaseMethodDeclarationSyntax GetWrappedBaseMethod(ref bool? changed) =>
            this.GetWrapped<ConstructorDeclarationSyntax>(ref changed);

        private protected override void SetSyntaxImpl(Roslyn::SyntaxNode newSyntax)
        {
            Init((ConstructorDeclarationSyntax)newSyntax);
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