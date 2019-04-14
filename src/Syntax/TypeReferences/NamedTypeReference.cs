using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RoslynSyntaxFactory = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Roslyn = Microsoft.CodeAnalysis;

namespace CSharpE.Syntax
{
    public sealed class NamedTypeReference : TypeReference, IEquatable<NamedTypeReference>
    {
        private TypeSyntax syntax;

        public NamedTypeReference(NamedTypeReference openGenericType, params TypeReference[] typeParameters) 
            : this(openGenericType.Namespace, openGenericType.Container, openGenericType.Name, typeParameters) { }

        public NamedTypeReference(string ns, string name, params TypeReference[] typeParameters)
            : this(ns, name, typeParameters.AsEnumerable()) { }

        public NamedTypeReference(string ns, string name, IEnumerable<TypeReference> typeParameters = null)
            : this(ns, null, name, typeParameters) { }

        public NamedTypeReference(string ns, NamedTypeReference container, string name, IEnumerable<TypeReference> typeParameters = null)
        {
            this.ns = ns;
            this.container = container;
            this.name = name ?? throw new ArgumentNullException(nameof(name), "Name cannot be null.");
            this.typeParameters =
                new TypeList(typeParameters ?? Array.Empty<TypeReference>(), this);
            isKnownType = true;
        }

        private static string StripGenericArity(string name)
        {
            var backtickIndex = name.IndexOf('`');

            if (backtickIndex == -1)
                return name;

            return name.Substring(0, backtickIndex);
        }

        public NamedTypeReference(Type type)
            : this(
                type.Namespace, type.DeclaringType, StripGenericArity(type.Name),
                type.GenericTypeArguments.Select(a => (TypeReference)a)) { }

        internal NamedTypeReference(TypeSyntax syntax, SyntaxNode parent)
        {
            Debug.Assert(syntax is PredefinedTypeSyntax || syntax is NameSyntax);

            this.syntax = syntax;
            Parent = parent;
        }

        public NamedTypeReference(INamedTypeSymbol symbol) => Resolve(symbol);

        private void Resolve()
        {
            if (isKnownType != null)
                return;

            // PERF: don't need semantics for PredefinedTypeSyntax

            if (SourceFile == null)
                throw new InvalidOperationException("Can't get this information for node without ancestor SourceFile.");

            var symbol = SourceFile.SyntaxContext.Resolve((TypeSyntax)GetSourceFileNode());

            Resolve(symbol);
        }

        private void Resolve(INamedTypeSymbol symbol)
        {
            if (symbol == null || symbol.TypeKind == TypeKind.Error)
            {
                SimpleNameSyntax nameSyntax;

                if (syntax is QualifiedNameSyntax qualifiedName)
                {
                    container = new NamedTypeReference(qualifiedName.Left, this);
                    nameSyntax = qualifiedName.Right;
                }
                else
                {
                    container = null;
                    nameSyntax = (SimpleNameSyntax)syntax;
                }
                syntaxName = symbol?.Name ?? nameSyntax.Identifier.ValueText;

                isKnownType = false;
            }
            else
            {
                syntaxNamespace = symbol.ContainingNamespace?.IsGlobalNamespace == true
                    ? string.Empty
                    : symbol.ContainingNamespace?.ToDisplayString();
                container = symbol.ContainingType == null ? null : new NamedTypeReference(symbol.ContainingType);
                syntaxName = symbol.Name;
                isKnownType = true;
            }
        }

        private bool? isKnownType;
        /// <summary>
        /// Indicates whether this is a known type.
        /// A known type has <see cref="Namespace"/>, <see cref="Container"/> (its containing type),
        /// <see cref="Name"/> and <see cref="TypeParameters"/>.
        /// An unknown type has <see cref="Container"/> (its containing type or namespace),
        /// <see cref="Name"/> and <see cref="TypeParameters"/>.
        /// </summary>
        private bool IsKnownType
        {
            get
            {
                Resolve();

                return isKnownType.Value;
            }
        }

        private string syntaxNamespace;
        private string ns;
        public string Namespace
        {
            get
            {
                Resolve();

                return ns ?? syntaxNamespace;
            }
            set
            {
                Resolve();

                if (isKnownType == false)
                    throw new InvalidOperationException();

                ns = value;
            }
        }

        private NamedTypeReference container;
        public NamedTypeReference Container
        {
            get
            {
                Resolve();

                return container;
            }
            set
            {
                Resolve();

                container = value;
            }
        }

        private string syntaxName;
        private string name;
        public string Name
        {
            get
            {
                // PERF: it should be possible to get name without semantics
                Resolve();

                return name ?? syntaxName;
            }
            set
            {
                Resolve();

                name = value;
            }
        }

        private TypeList typeParameters;
        public IList<TypeReference> TypeParameters
        {
            get
            {
                if (typeParameters == null)
                {
                    var genericSyntax = syntax as GenericNameSyntax;

                    typeParameters = new TypeList(
                        genericSyntax?.TypeArgumentList.Arguments ?? default, this);
                }

                return typeParameters;
            }
            set => SetList(ref typeParameters, new TypeList(value, this));
        }

        internal override StringBuilder ComputeFullName(StringBuilder stringBuilder)
        {
            if (Namespace != null)
            {
                stringBuilder.Append(Namespace);
                stringBuilder.Append('.');
            }

            if (Container != null)
            {
                Container.ComputeFullName(stringBuilder);
                stringBuilder.Append('.');
            }

            stringBuilder.Append(Name);

            if (TypeParameters.Any())
            {
                stringBuilder.Append('<');

                bool first = true;

                foreach (var typeParameter in TypeParameters)
                {
                    if (!first)
                        stringBuilder.Append(", ");

                    typeParameter.ComputeFullName(stringBuilder);

                    first = false;
                }

                stringBuilder.Append('>');
            }

            return stringBuilder;
        }

        public string FullName => ComputeFullName(new StringBuilder()).ToString();

        private static bool IsAncestorNamespace(string parent, string child) => parent == child || child.StartsWith(parent + '.');

        public bool RequiresUsingNamespace => IsKnownType && Namespace != null && !IsPredefinedType &&
                                              !IsAncestorNamespace(Namespace, EnclosingType.GetNamespace());

        public static implicit operator NamedTypeReference(Type type) => type == null ? null : new NamedTypeReference(type);

        private protected override TypeSyntax GetWrappedType(ref bool? changed)
        {
            GetAndResetChanged(ref changed);

            bool? thisChanged = false;

            var newTypeParameters = typeParameters?.GetWrapped(ref thisChanged) ??
                                    (syntax as GenericNameSyntax)?.TypeArgumentList.Arguments ?? default;

            // if Resolve() wasn't called, only type parameters could have been changed
            if (isKnownType == null && thisChanged == false)
            {
                if (!IsAnnotated(syntax))
                {
                    syntax = Annotate(syntax);

                    SetChanged(ref changed);
                }

                return syntax;
            }

            // TODO: don't add the namespace we're currently in
            if (RequiresUsingNamespace)
                SourceFile?.EnsureUsingNamespace(Namespace);

            var newNamespace = ns ?? syntaxNamespace;
            var newContainer = container?.GetWrapped(ref thisChanged);
            var newName = name ?? syntaxName;

            if (syntax == null || thisChanged == true || newNamespace != syntaxNamespace || newName != syntaxName ||
                !IsAnnotated(syntax))
            {
                var predefinedType = GetPredefinedType();
                if (predefinedType != null)
                {
                    syntax = predefinedType;
                }
                else
                {
                    SimpleNameSyntax simpleName;
                    if (newTypeParameters.Any())
                    {
                        simpleName = RoslynSyntaxFactory.GenericName(
                            RoslynSyntaxFactory.Identifier(Name),
                            RoslynSyntaxFactory.TypeArgumentList(newTypeParameters));
                    }
                    else
                    {
                        simpleName = RoslynSyntaxFactory.IdentifierName(Name);
                    }

                    if (newContainer == null)
                        syntax = simpleName;
                    else
                        syntax = RoslynSyntaxFactory.QualifiedName((NameSyntax)newContainer, simpleName);
                }

                syntax = Annotate(syntax);

                SetChanged(ref changed);

                syntaxNamespace = newNamespace;
                syntaxName = newName;
            }

            return syntax;
        }

        private protected override void SetSyntaxImpl(Roslyn::SyntaxNode newSyntax)
        {
            syntax = (TypeSyntax)newSyntax;
            typeParameters = null;
            isKnownType = false;
        }

        private bool IsPredefinedType => GetPredefinedSyntaxKind() != SyntaxKind.None;

        private PredefinedTypeSyntax GetPredefinedType()
        {
            var kind = GetPredefinedSyntaxKind();

            if (kind == SyntaxKind.None)
                return null;

            return RoslynSyntaxFactory.PredefinedType(RoslynSyntaxFactory.Token(kind));
        }

        private SyntaxKind GetPredefinedSyntaxKind()
        {
            if (Namespace != nameof(System) || Container != null || TypeParameters.Any())
                return SyntaxKind.None;

            switch (Name)
            {
                case nameof(Boolean):
                    return SyntaxKind.BoolKeyword;
                case nameof(Byte):
                    return SyntaxKind.ByteKeyword;
                case nameof(SByte):
                    return SyntaxKind.SByteKeyword;
                case nameof(Int32):
                    return SyntaxKind.IntKeyword;
                case nameof(UInt32):
                    return SyntaxKind.UIntKeyword;
                case nameof(Int16):
                    return SyntaxKind.ShortKeyword;
                case nameof(UInt16):
                    return SyntaxKind.UShortKeyword;
                case nameof(Int64):
                    return SyntaxKind.LongKeyword;
                case nameof(UInt64):
                    return SyntaxKind.ULongKeyword;
                case nameof(Single):
                    return SyntaxKind.FloatKeyword;
                case nameof(Double):
                    return SyntaxKind.DoubleKeyword;
                case nameof(Decimal):
                    return SyntaxKind.DecimalKeyword;
                case nameof(String):
                    return SyntaxKind.StringKeyword;
                case nameof(Char):
                    return SyntaxKind.CharKeyword;
                case nameof(Object):
                    return SyntaxKind.ObjectKeyword;
                case "Void": // nameof(Void) is illegal
                    return SyntaxKind.VoidKeyword;
                default:
                    return SyntaxKind.None;
            }
        }

        public override string ToString() => FullName;

        internal override SyntaxNode Parent { get; set; }

        internal override SyntaxNode Clone() => new NamedTypeReference(Namespace, Container, Name, TypeParameters);

        public bool Equals(NamedTypeReference other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;

            var ordinalComparer = StringComparer.Ordinal;

            return ordinalComparer.Equals(Namespace, other.Namespace) && Equals(Container, other.Container) &&
                   ordinalComparer.Equals(Name, other.Name) && TypeParameters.SequenceEqual(other.TypeParameters);
        }

        public override bool Equals(object obj) => Equals(obj as NamedTypeReference);

        public override int GetHashCode()
        {
            var ordinalComparer = StringComparer.Ordinal;
            
            var hc = new HashCode();
            hc.Add(Namespace, ordinalComparer);
            hc.Add(Container);
            hc.Add(Name, ordinalComparer);
            foreach (var tp in TypeParameters)
                hc.Add(tp);
            return hc.ToHashCode();
        }
    }
}