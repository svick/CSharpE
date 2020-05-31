using System;
using System.Linq;
using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static CSharpE.Syntax.MemberModifiers;
using RoslynSyntaxFactory = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Roslyn = Microsoft.CodeAnalysis;

namespace CSharpE.Syntax
{
    public sealed class EventDefinition : MemberDefinition
    {
        private const MemberModifiers ValidModifiers =
            AccessModifiersMask | New | Static | Virtual | Sealed | Override | Abstract | Extern | Unsafe;

        private protected override void ValidateModifiers(MemberModifiers value)
        {
            var invalidModifiers = value & ~ValidModifiers;
            if (invalidModifiers != 0)
                throw new ArgumentException($"The modifiers {invalidModifiers} are not valid for an event.", nameof(value));
        }

        private MemberDeclarationSyntax syntax;

        private protected override MemberDeclarationSyntax MemberSyntax => syntax;

        private T SyntaxSwitch<T>(Func<EventDeclarationSyntax, T> eventFunc, Func<EventFieldDeclarationSyntax, T> eventFieldFunc) =>
            syntax switch
            {
                EventDeclarationSyntax eventDeclaration => eventFunc(eventDeclaration),
                EventFieldDeclarationSyntax eventFieldDeclaration => eventFieldFunc(eventFieldDeclaration),
                _ => throw new InvalidOperationException(syntax.GetType().Name),
            };

        internal EventDefinition(MemberDeclarationSyntax syntax, TypeDefinition parent)
            : base(syntax)
        {
            switch (syntax)
            {
                case EventDeclarationSyntax _:
                    break;
                case EventFieldDeclarationSyntax eventField:
                    if (eventField.Declaration.Variables.Count > 1)
                        throw new ArgumentException(
                            "EventFieldDeclarationSyntax with more than one variable is not supported here.", nameof(syntax));
                    break;
                default:
                    throw new ArgumentException(syntax.GetType().Name);
            }

            Init(syntax);
            Parent = parent;
        }

        private void Init(MemberDeclarationSyntax syntax)
        {
            this.syntax = syntax;

            Modifiers = FromRoslyn.MemberModifiers(syntax.Modifiers);
            name = new Identifier(SyntaxSwitch(e => e.Identifier, e => e.Declaration.Variables.Single().Identifier));
        }

        public EventDefinition(MemberModifiers modifiers, TypeReference type, string name)
            : this(modifiers, type, null, name) { }

        public EventDefinition(MemberModifiers modifiers, TypeReference type, NamedTypeReference explicitInterface, string name)
            : this(modifiers, type, explicitInterface, name, null, null) { }

        public EventDefinition(
            MemberModifiers modifiers, TypeReference type, string name, AccessorDefinition addAccessor, AccessorDefinition removeAccessor)
            : this(modifiers, type, null, name, addAccessor, removeAccessor) { }

        public EventDefinition(
            MemberModifiers modifiers, TypeReference type, NamedTypeReference explicitInterface, string name,
            AccessorDefinition addAccessor, AccessorDefinition removeAccessor)
        {
            Modifiers = modifiers;
            Type = type;
            ExplicitInterface = explicitInterface;
            Name = name;
            AddAccessor = addAccessor;
            RemoveAccessor = removeAccessor;
        }

        public bool IsNew
        {
            get => Modifiers.Contains(New);
            set => Modifiers = Modifiers.With(New, value);
        }

        public bool IsStatic
        {
            get => Modifiers.Contains(Static);
            set => Modifiers = Modifiers.With(Static, value);
        }

        public bool IsVirtual
        {
            get => Modifiers.Contains(Virtual);
            set => Modifiers = Modifiers.With(Virtual, value);
        }

        public bool IsSealed
        {
            get => Modifiers.Contains(Sealed);
            set => Modifiers = Modifiers.With(Sealed, value);
        }

        public bool IsOverride
        {
            get => Modifiers.Contains(Override);
            set => Modifiers = Modifiers.With(Override, value);
        }

        public bool IsAbstract
        {
            get => Modifiers.Contains(Abstract);
            set => Modifiers = Modifiers.With(Abstract, value);
        }

        public bool IsExtern
        {
            get => Modifiers.Contains(Extern);
            set => Modifiers = Modifiers.With(Extern, value);
        }

        public bool IsUnsafe
        {
            get => Modifiers.Contains(Unsafe);
            set => Modifiers = Modifiers.With(Unsafe, value);
        }

        private TypeReference type;
        public TypeReference Type
        {
            get
            {
                if (type == null)
                    type = FromRoslyn.TypeReference(SyntaxSwitch(e => e.Type, e => e.Declaration.Type), this);

                return type;
            }
            set => SetNotNull(ref type, value);
        }

        private bool explicitInterfaceSet;
        private NamedTypeReference explicitInterface;
        public NamedTypeReference ExplicitInterface
        {
            get
            {
                if (!explicitInterfaceSet)
                {
                    var explicitInterfaceSpecifier = SyntaxSwitch(e => e.ExplicitInterfaceSpecifier, e => null);
                    explicitInterface = explicitInterfaceSpecifier == null
                        ? null
                        : new NamedTypeReference(explicitInterfaceSpecifier.Name, this);
                    explicitInterfaceSet = true;
                }

                return explicitInterface;
            }
            set
            {
                Set(ref explicitInterface, value);
                explicitInterfaceSet = true;
            }
        }

        private Identifier name;
        public string Name
        {
            get => name.Text;
            set => name.Text = value;
        }

        // TODO: events with more than one of each kind of accessor should probably be error nodes
        private AccessorDeclarationSyntax FindAccessor(SyntaxKind accessorKind) =>
            SyntaxSwitch(e => e.AccessorList?.Accessors.FirstOrDefault(a => a.IsKind(accessorKind)), e => null);

        private bool addAccessorSet;
        private AccessorDefinition addAccessor;
        public AccessorDefinition AddAccessor
        {
            get
            {
                if (!addAccessorSet)
                {
                    var declaration = FindAccessor(SyntaxKind.AddAccessorDeclaration);

                    if (declaration != null)
                        addAccessor = new AccessorDefinition(declaration, this);

                    addAccessorSet = true;
                }
                return addAccessor;
            }
            set
            {
                Set(ref addAccessor, value);
                if (addAccessor != null)
                    addAccessor.Kind = SyntaxKind.AddAccessorDeclaration;
                addAccessorSet = true;
            }
        }

        private bool removeAccessorSet;
        private AccessorDefinition removeAccessor;
        public AccessorDefinition RemoveAccessor
        {
            get
            {
                if (!removeAccessorSet)
                {
                    var declaration = FindAccessor(SyntaxKind.RemoveAccessorDeclaration);

                    if (declaration != null)
                        removeAccessor = new AccessorDefinition(declaration, this);

                    removeAccessorSet = true;
                }
                return removeAccessor;
            }
            set
            {
                Set(ref removeAccessor, value);
                if (removeAccessor != null)
                    removeAccessor.Kind = SyntaxKind.RemoveAccessorDeclaration;
                removeAccessorSet = true;
            }
        }

        private protected override MemberDeclarationSyntax GetWrappedMember(ref bool? changed)
        {
            GetAndResetChanged(ref changed);

            bool? thisChanged = false;

            var newAttributes = attributes?.GetWrapped(ref thisChanged) ?? syntax?.AttributeLists ?? default;
            var newModifiers = Modifiers;
            var newType = type?.GetWrapped(ref thisChanged) ?? SyntaxSwitch(e => e.Type, e => e.Declaration.Type);
            var newExplicitInterface = explicitInterfaceSet
                ? explicitInterface?.GetWrappedName(ref thisChanged)
                : SyntaxSwitch(e => e.ExplicitInterfaceSpecifier?.Name, e => null);
            var newName = name.GetWrapped(ref thisChanged);
            var newAddAccessor = addAccessorSet
                ? addAccessor?.GetWrapped(ref thisChanged)
                : FindAccessor(SyntaxKind.AddAccessorDeclaration);
            var newRemoveAccessor = removeAccessorSet
                ? removeAccessor?.GetWrapped(ref thisChanged)
                : FindAccessor(SyntaxKind.RemoveAccessorDeclaration);

            if (syntax == null || FromRoslyn.MemberModifiers(syntax.Modifiers) != newModifiers || thisChanged == true ||
                ShouldAnnotate(syntax, changed))
            {
                MemberDeclarationSyntax newSyntax;

                if (newAddAccessor == null && newRemoveAccessor == null)
                {
                    if (newExplicitInterface != null)
                        throw new InvalidOperationException("Events without accessors can't be explicit interface implementations.");

                    newSyntax = RoslynSyntaxFactory.EventFieldDeclaration(
                        newAttributes, newModifiers.GetWrapped(), RoslynSyntaxFactory.VariableDeclaration(
                            newType, RoslynSyntaxFactory.SingletonSeparatedList(RoslynSyntaxFactory.VariableDeclarator(newName))));
                }
                else
                {
                    var explicitInterfaceSpecifier = newExplicitInterface == null
                        ? null
                        : RoslynSyntaxFactory.ExplicitInterfaceSpecifier(newExplicitInterface);
                    var accessors =
                        RoslynSyntaxFactory.List(new[] { newAddAccessor, newRemoveAccessor }.Where(a => a != null));

                    newSyntax = RoslynSyntaxFactory.EventDeclaration(
                        newAttributes, newModifiers.GetWrapped(), newType, explicitInterfaceSpecifier, newName,
                        RoslynSyntaxFactory.AccessorList(accessors));
                }

                syntax = Annotate(newSyntax);

                SetChanged(ref changed);
            }

            return syntax;
        }

        private protected override void SetSyntaxImpl(Roslyn::SyntaxNode newSyntax)
        {
            Init((MemberDeclarationSyntax)newSyntax);

            SetList(ref attributes, null);
            Set(ref type, null);
            Set(ref explicitInterface, null);
            explicitInterfaceSet = false;
            Set(ref addAccessor, null);
            addAccessorSet = false;
            Set(ref removeAccessor, null);
            removeAccessorSet = false;
        }

        private protected override SyntaxNode CloneImpl() =>
            new EventDefinition(Modifiers, Type, ExplicitInterface, Name, AddAccessor, RemoveAccessor)
            {
                Attributes = Attributes
            };

        protected override void ReplaceExpressionsImpl<T>(Func<T, bool> filter, Func<T, Expression> projection)
        {
            AddAccessor?.ReplaceExpressions(filter, projection);
            RemoveAccessor?.ReplaceExpressions(filter, projection);
        }
    }
}
