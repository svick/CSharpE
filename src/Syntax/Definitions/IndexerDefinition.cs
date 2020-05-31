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
    public sealed class IndexerDefinition : BasePropertyDefinition, ISyntaxWrapper<IndexerDeclarationSyntax>
    {
        private IndexerDeclarationSyntax syntax;

        private protected override BasePropertyDeclarationSyntax BasePropertySyntax => syntax;

        internal IndexerDefinition(IndexerDeclarationSyntax syntax, TypeDefinition parent)
            : base(syntax)
        {
            Init(syntax);
            Parent = parent;
        }

        private void Init(IndexerDeclarationSyntax syntax)
        {
            this.syntax = syntax;
            
            Modifiers = FromRoslyn.MemberModifiers(syntax.Modifiers);
        }

        public IndexerDefinition(
            MemberModifiers modifiers, TypeReference type, IEnumerable<Parameter> parameters,
            AccessorDefinition getAccessor, AccessorDefinition setAccessor)
            : this(modifiers, type, null, parameters, getAccessor, setAccessor) { }

        public IndexerDefinition(
            MemberModifiers modifiers, TypeReference type, NamedTypeReference explicitInterface, IEnumerable<Parameter> parameters,
            AccessorDefinition getAccessor, AccessorDefinition setAccessor)
        {
            Modifiers = modifiers;
            Type = type;
            ExplicitInterface = explicitInterface;
            Parameters = parameters?.ToList();
            GetAccessor = getAccessor;
            SetAccessor = setAccessor;
        }

        private const MemberModifiers ValidModifiers =
            AccessModifiersMask | New | Virtual | Sealed | Override | Abstract | Extern | Unsafe;

        private protected override void ValidateModifiers(MemberModifiers value)
        {
            var invalidModifiers = value & ~ValidModifiers;
            if (invalidModifiers != 0)
                throw new ArgumentException(
                    $"The modifiers {invalidModifiers} are not valid for a property.", nameof(value));
        }

        private SeparatedSyntaxList<Parameter, ParameterSyntax> parameters;
        public IList<Parameter> Parameters
        {
            get
            {
                if (parameters == null)
                    parameters = new SeparatedSyntaxList<Parameter, ParameterSyntax>(
                        syntax.ParameterList.Parameters, this);

                return parameters;
            }
            set => SetList(ref parameters, new SeparatedSyntaxList<Parameter, ParameterSyntax>(value, this));
        }

        // TODO: expression body

        IndexerDeclarationSyntax ISyntaxWrapper<IndexerDeclarationSyntax>.GetWrapped(ref bool? changed)
        {
            GetAndResetChanged(ref changed);

            bool? thisChanged = false;

            var newAttributes = attributes?.GetWrapped(ref thisChanged) ?? syntax?.AttributeLists ?? default;
            var newModifiers = Modifiers;
            var newType = type?.GetWrapped(ref thisChanged) ?? syntax.Type;
            var newExplicitInterface = explicitInterfaceSet
                ? explicitInterface?.GetWrappedName(ref thisChanged)
                : syntax.ExplicitInterfaceSpecifier?.Name;
            var newParameters = parameters?.GetWrapped(ref thisChanged) ?? syntax.ParameterList.Parameters;
            var newGetAccessor = getAccessorSet
                ? getAccessor?.GetWrapped(ref thisChanged)
                : FindAccessor(SyntaxKind.GetAccessorDeclaration);
            var newSetAccessor = setAccessorSet
                ? setAccessor?.GetWrapped(ref thisChanged)
                : FindAccessor(SyntaxKind.SetAccessorDeclaration);

            if (syntax == null || newModifiers != FromRoslyn.MemberModifiers(syntax.Modifiers) ||
                thisChanged == true || ShouldAnnotate(syntax, changed))
            {
                var explicitInterfaceSpecifier = newExplicitInterface == null
                    ? null
                    : RoslynSyntaxFactory.ExplicitInterfaceSpecifier(newExplicitInterface);
                var accessors =
                    RoslynSyntaxFactory.List(new[] { newGetAccessor, newSetAccessor }.Where(a => a != null));

                var newSyntax = RoslynSyntaxFactory.IndexerDeclaration(
                    newAttributes, newModifiers.GetWrapped(), newType, explicitInterfaceSpecifier,
                    RoslynSyntaxFactory.BracketedParameterList(newParameters), RoslynSyntaxFactory.AccessorList(accessors));

                syntax = Annotate(newSyntax);

                SetChanged(ref changed);
            }

            return syntax;
        }

        private protected override BasePropertyDeclarationSyntax GetWrappedBaseProperty(ref bool? changed) =>
            this.GetWrapped<IndexerDeclarationSyntax>(ref changed);

        private protected override void SetSyntaxImpl(Roslyn::SyntaxNode newSyntax)
        {
            Init((IndexerDeclarationSyntax)newSyntax);
            SetList(ref attributes, null);
            Set(ref type, null);
            Set(ref explicitInterface, null);
            explicitInterfaceSet = false;
            SetList(ref parameters, null);
            Set(ref getAccessor, null);
            getAccessorSet = false;
            Set(ref setAccessor, null);
            setAccessorSet = false;
        }

        private protected override SyntaxNode CloneImpl() =>
            new IndexerDefinition(Modifiers, Type, ExplicitInterface, Parameters, GetAccessor, SetAccessor)
            {
                Attributes = Attributes
            };
    }
}