using System;
using System.Collections.Generic;
using System.Linq;
using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static CSharpE.Syntax.MemberModifiers;
using RoslynSyntaxFactory = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Roslyn = Microsoft.CodeAnalysis;

namespace CSharpE.Syntax
{
    public sealed class ConstructorDefinition : BaseMethodDefinition, ISyntaxWrapper<ConstructorDeclarationSyntax>
    {
        private ConstructorDeclarationSyntax syntax;

        private protected override BaseMethodDeclarationSyntax BaseMethodSyntax => syntax;

        internal ConstructorDefinition(ConstructorDeclarationSyntax syntax, TypeDefinition parent)
            : base(syntax)
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

            var newAttributes = attributes?.GetWrapped(ref thisChanged) ?? syntax?.AttributeLists ?? default;
            var newModifiers = Modifiers;
            var newParameters = parameters?.GetWrapped(ref thisChanged) ?? syntax.ParameterList.Parameters;
            var newBody = bodySet ? body?.GetWrapped(ref thisChanged) : syntax.Body;

            if (syntax == null || newModifiers != FromRoslyn.MemberModifiers(syntax.Modifiers) ||
                thisChanged == true || ShouldAnnotate(syntax, changed))
            {
                if (Parent == null)
                    throw new InvalidOperationException("Can't create syntax node for constructor with no parent type.");

                var newSyntax = RoslynSyntaxFactory.ConstructorDeclaration(
                    newAttributes, newModifiers.GetWrapped(), RoslynSyntaxFactory.Identifier(ParentType.Name),
                    RoslynSyntaxFactory.ParameterList(newParameters), default, newBody);

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

            SetList(ref attributes, null);
            SetList(ref parameters, null);
            Set(ref body, null);
        }

        private protected override SyntaxNode CloneImpl() => new ConstructorDefinition(Modifiers, Parameters, Body.Statements);

        protected override void ReplaceExpressionsImpl<T>(Func<T, bool> filter, Func<T, Expression> projection)
        {
            // TODO: initializers

            base.ReplaceExpressionsImpl(filter, projection);
        }
    }
}