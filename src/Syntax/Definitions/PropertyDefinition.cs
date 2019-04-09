using System;
using System.Linq;
using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static CSharpE.Syntax.MemberModifiers;
using RoslynSyntaxFactory = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Roslyn = Microsoft.CodeAnalysis;

namespace CSharpE.Syntax
{
    public sealed class PropertyDefinition : BasePropertyDefinition, ISyntaxWrapper<PropertyDeclarationSyntax>
    {
        private PropertyDeclarationSyntax syntax;

        private protected override BasePropertyDeclarationSyntax BasePropertySyntax => syntax;

        public PropertyDefinition(PropertyDeclarationSyntax syntax, TypeDefinition parent)
        {
            Init(syntax);
            Parent = parent;
        }

        private void Init(PropertyDeclarationSyntax syntax)
        {
            this.syntax = syntax;
            
            Modifiers = FromRoslyn.MemberModifiers(syntax.Modifiers);
            name = new Identifier(syntax.Identifier);
        }
        
        public PropertyDefinition(MemberModifiers modifiers, TypeReference type, string name)
        {
            Modifiers = modifiers;
            Type = type;
            Name = name;
            getAccessorSet = true;
            setAccessorSet = true;
        }

        private const MemberModifiers ValidModifiers =
            AccessModifiersMask | New | Static | Virtual | Sealed | Override | Abstract | Extern | Unsafe;

        private protected override void ValidateModifiers(MemberModifiers value)
        {
            var invalidModifiers = value & ~ValidModifiers;
            if (invalidModifiers != 0)
                throw new ArgumentException(
                    $"The modifiers {invalidModifiers} are not valid for a property.", nameof(value));
        }
        
        public bool IsStatic
        {
            get => Modifiers.Contains(Static);
            set => Modifiers = Modifiers.With(Static, value);
        }
        
        private Identifier name;
        public string Name
        {
            get => name.Text;
            set => name.Text = value;
        }
        
        // TODO: expression body and initializer

        PropertyDeclarationSyntax ISyntaxWrapper<PropertyDeclarationSyntax>.GetWrapped(ref bool? changed)
        {
            GetAndResetChanged(ref changed);

            bool? thisChanged = false;

            var newAttributes = attributes?.GetWrapped(ref thisChanged) ?? syntax?.AttributeLists ?? default;
            var newModifiers = Modifiers;
            var newType = type?.GetWrapped(ref thisChanged) ?? syntax.Type;
            var newName = name.GetWrapped(ref thisChanged);
            var newGetAccessor = getAccessorSet
                ? getAccessor?.GetWrapped(ref thisChanged)
                : FindAccessor(SyntaxKind.GetAccessorDeclaration);
            var newSetAccessor = setAccessorSet
                ? setAccessor?.GetWrapped(ref thisChanged)
                : FindAccessor(SyntaxKind.SetAccessorDeclaration);

            if (syntax == null || newModifiers != FromRoslyn.MemberModifiers(syntax.Modifiers) ||
                thisChanged == true || !IsAnnotated(syntax))
            {
                var accessors = RoslynSyntaxFactory.List(new[] {newGetAccessor, newSetAccessor}.Where(a => a != null));

                var newSyntax = RoslynSyntaxFactory.PropertyDeclaration(
                    newAttributes, newModifiers.GetWrapped(), newType, null, newName,
                    RoslynSyntaxFactory.AccessorList(accessors));

                syntax = Annotate(newSyntax);

                SetChanged(ref changed);
            }

            return syntax;
        }

        private protected override BasePropertyDeclarationSyntax GetWrappedBaseProperty(ref bool? changed) =>
            this.GetWrapped<PropertyDeclarationSyntax>(ref changed);

        private protected override void SetSyntaxImpl(Roslyn::SyntaxNode newSyntax)
        {
            throw new NotImplementedException();
        }

        internal override SyntaxNode Clone()
        {
            throw new NotImplementedException();
        }
    }
}