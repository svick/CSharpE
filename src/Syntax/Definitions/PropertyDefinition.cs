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

        internal PropertyDefinition(PropertyDeclarationSyntax syntax, TypeDefinition parent)
            : base(syntax)
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

        public PropertyDefinition(
            MemberModifiers modifiers, TypeReference type, string name,
            AccessorDefinition getAccessor, AccessorDefinition setAccessor, Expression initializer = null)
            : this(modifiers, type, null, name, getAccessor, setAccessor, initializer) { }

        public PropertyDefinition(
            MemberModifiers modifiers, TypeReference type, NamedTypeReference explicitInterface, string name, 
            AccessorDefinition getAccessor, AccessorDefinition setAccessor, Expression initializer = null)
        {
            Modifiers = modifiers;
            Type = type;
            ExplicitInterface = explicitInterface;
            Name = name;
            GetAccessor = getAccessor;
            SetAccessor = setAccessor;
            Initializer = initializer;
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

        private protected override ArrowExpressionClauseSyntax GetExpressionBody() => syntax.ExpressionBody;

        private bool initializerSet;
        private Expression initializer;
        public Expression Initializer
        {
            get
            {
                if (!initializerSet)
                {
                    var syntaxInitializer = syntax.Initializer;

                    if (syntaxInitializer != null)
                        initializer = FromRoslyn.Expression(syntaxInitializer.Value, this);

                    initializerSet = true;
                }

                return initializer;
            }
            set
            {
                Set(ref initializer, value);
                initializerSet = true;
            }
        }

        PropertyDeclarationSyntax ISyntaxWrapper<PropertyDeclarationSyntax>.GetWrapped(ref bool? changed)
        {
            GetAndResetChanged(ref changed, out var thisChanged);

            var newAttributes = attributes?.GetWrapped(ref thisChanged) ?? syntax?.AttributeLists ?? default;
            var newModifiers = Modifiers;
            var newType = type?.GetWrapped(ref thisChanged) ?? syntax.Type;
            var newExplicitInterface = explicitInterfaceSet
                ? explicitInterface?.GetWrappedName(ref thisChanged)
                : syntax.ExplicitInterfaceSpecifier?.Name;
            var newName = name.GetWrapped(ref thisChanged);
            var newGetAccessor = getAccessorSet
                ? getAccessor?.GetWrapped(ref thisChanged)
                : FindAccessor(SyntaxKind.GetAccessorDeclaration);
            var newSetAccessor = setAccessorSet
                ? setAccessor?.GetWrapped(ref thisChanged)
                : FindAccessor(SyntaxKind.SetAccessorDeclaration);
            var newInitializer = initializerSet ? initializer?.GetWrapped(ref thisChanged) : syntax.Initializer?.Value;

            if (syntax == null || newModifiers != FromRoslyn.MemberModifiers(syntax.Modifiers) ||
                thisChanged == true || ShouldAnnotate(syntax, changed))
            {
                var explicitInterfaceSpecifier = newExplicitInterface == null
                    ? null
                    : RoslynSyntaxFactory.ExplicitInterfaceSpecifier(newExplicitInterface);
                var accessors =
                    RoslynSyntaxFactory.List(new[] { newGetAccessor, newSetAccessor }.Where(a => a != null));
                var equalsValueClause =
                    newInitializer == null ? null : RoslynSyntaxFactory.EqualsValueClause(newInitializer);
                var semicolon = equalsValueClause != null ? RoslynSyntaxFactory.Token(SyntaxKind.SemicolonToken) : default;

                var newSyntax = RoslynSyntaxFactory.PropertyDeclaration(
                    newAttributes, newModifiers.GetWrapped(), newType, explicitInterfaceSpecifier, newName,
                    RoslynSyntaxFactory.AccessorList(accessors), null, equalsValueClause, semicolon);

                syntax = Annotate(newSyntax);

                SetChanged(ref changed);
            }

            return syntax;
        }

        private protected override BasePropertyDeclarationSyntax GetWrappedBaseProperty(ref bool? changed) =>
            this.GetWrapped<PropertyDeclarationSyntax>(ref changed);

        private protected override void SetSyntaxImpl(Roslyn::SyntaxNode newSyntax)
        {
            Init((PropertyDeclarationSyntax)newSyntax);
            SetList(ref attributes, null);
            Set(ref type, null);
            Set(ref explicitInterface, null);
            explicitInterfaceSet = false;
            Set(ref getAccessor, null);
            getAccessorSet = false;
            Set(ref setAccessor, null);
            setAccessorSet = false;
            Set(ref initializer, null);
            initializerSet = false;
        }

        private protected override SyntaxNode CloneImpl() =>
            new PropertyDefinition(Modifiers, Type, ExplicitInterface, Name, GetAccessor, SetAccessor, Initializer)
            {
                Attributes = Attributes
            };

        protected override void ReplaceExpressionsImpl<T>(Func<T, bool> filter, Func<T, Expression> projection)
        {
            base.ReplaceExpressionsImpl(filter, projection);
            Initializer = Expression.ReplaceExpressions(Initializer, filter, projection);
        }
    }
}