using System;
using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static CSharpE.Syntax.MemberModifiers;
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
            throw new NotImplementedException();
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