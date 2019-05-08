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
    public sealed class NamedTypeReference : TypeReference
    {
        private TypeSyntax syntax;

        public NamedTypeReference(NamedTypeReference openGenericType, params TypeReference[] typeArguments) 
            : this(openGenericType.Namespace, openGenericType.Container, openGenericType.Name, typeArguments) { }

        public NamedTypeReference(string ns, string name, params TypeReference[] typeArguments)
            : this(ns, name, typeArguments.AsEnumerable()) { }

        public NamedTypeReference(string ns, string name, IEnumerable<TypeReference> typeArguments = null)
            : this(ns, null, name, typeArguments) { }

        public NamedTypeReference(string ns, NamedTypeReference container, string name, IEnumerable<TypeReference> typeArguments = null)
        {
            this.ns = ns;
            this.container = container;
            this.name = name ?? throw new ArgumentNullException(nameof(name), "Name cannot be null.");
            this.typeArguments =
                new TypeList(typeArguments ?? Array.Empty<TypeReference>(), this);
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
            : base(syntax)
        {
            Debug.Assert(syntax is PredefinedTypeSyntax || syntax is NameSyntax);

            this.syntax = syntax;
            Parent = parent;
        }

        internal NamedTypeReference(INamedTypeSymbol symbol) => Resolve(symbol);

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

                syntaxName = symbol?.Name;
                if (string.IsNullOrEmpty(syntaxName))
                    syntaxName = nameSyntax.Identifier.ValueText;

                isKnownType = false;
            }
            else
            {
                syntaxNamespace = symbol.ContainingNamespace?.IsGlobalNamespace == true
                    ? null
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
        /// <see cref="Name"/> and <see cref="TypeArguments"/>.
        /// An unknown type has <see cref="Container"/> (its containing type or namespace),
        /// <see cref="Name"/> and <see cref="TypeArguments"/>.
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
                if (value == string.Empty)
                    throw new ArgumentException("Namespace can't be empty, use null for the global namespace.", nameof(value));

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
                if (string.IsNullOrEmpty(value))
                    throw new ArgumentException("Name can't be null or empty.", nameof(value));

                Resolve();

                name = value;
            }
        }

        private TypeList typeArguments;
        public IList<TypeReference> TypeArguments
        {
            get
            {
                if (typeArguments == null)
                {
                    var genericSyntax = syntax as GenericNameSyntax;

                    typeArguments = new TypeList(
                        genericSyntax?.TypeArgumentList.Arguments ?? default, this);
                }

                return typeArguments;
            }
            set => SetList(ref typeArguments, new TypeList(value, this));
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

            if (TypeArguments.Any())
            {
                stringBuilder.Append('<');

                bool first = true;

                foreach (var typeParameter in TypeArguments)
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

        private static bool IsAncestorNamespace(string parent, string child) => parent == child || child.StartsWith(parent + '.');

        public static implicit operator NamedTypeReference(Type type) => type == null ? null : new NamedTypeReference(type);

        private protected override TypeSyntax GetWrappedType(ref bool? changed)
        {
            GetAndResetChanged(ref changed);

            bool? thisChanged = false;

            var newTypeArguments = typeArguments?.GetWrapped(ref thisChanged) ??
                                    (syntax as GenericNameSyntax)?.TypeArgumentList.Arguments ?? default;

            TypeArgumentListSyntax GetTypeArguments(SeparatedSyntaxList<TypeSyntax> arguments)
            {
                if (arguments.Contains(null))
                    arguments = RoslynSyntaxFactory.SeparatedList(
                        newTypeArguments.Select(l => l ?? RoslynSyntaxFactory.OmittedTypeArgument()));

                return RoslynSyntaxFactory.TypeArgumentList(arguments);
            }

            // if Resolve() wasn't called, only type parameters could have been changed
            if (isKnownType == null)
            {
                // either old and new are both non-generic or both are generic
                if (newTypeArguments.Any() == syntax is GenericNameSyntax)
                {
                    if (thisChanged == true || !IsAnnotated(syntax))
                    {
                        if (thisChanged == true)
                        {
                            // if type parameters changed, but genericity didn't, syntax must be generic
                            var genericSyntax = (GenericNameSyntax)syntax;

                            syntax = genericSyntax.WithTypeArgumentList(GetTypeArguments(newTypeArguments));
                        }

                        syntax = Annotate(syntax);

                        SetChanged(ref changed);
                    }

                    return syntax;
                }
            }

            bool requiresUsingNamespace = IsKnownType && Namespace != null && !IsPredefinedType &&
                                         !IsAncestorNamespace(Namespace, EnclosingType.GetNamespace());
            if (requiresUsingNamespace)
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
                    if (newTypeArguments.Any())
                    {
                        simpleName = RoslynSyntaxFactory.GenericName(
                            RoslynSyntaxFactory.Identifier(Name), GetTypeArguments(newTypeArguments));
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
            typeArguments = null;
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
            if (Namespace != nameof(System) || Container != null || TypeArguments.Any())
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

        private protected override SyntaxNode CloneImpl() => new NamedTypeReference(Namespace, Container, Name, TypeArguments);

        public override IEnumerable<SyntaxNode> GetChildren() => TypeArguments;

        public override bool Equals(TypeReference other)
        {
            if (ReferenceEquals(this, other)) return true;
            if (!(other is NamedTypeReference otherNamed)) return false;

            var ordinalComparer = StringComparer.Ordinal;

            return ordinalComparer.Equals(Namespace, otherNamed.Namespace) &&
                   Equals(Container, otherNamed.Container) &&
                   ordinalComparer.Equals(Name, otherNamed.Name) &&
                   TypeArguments.SequenceEqual(otherNamed.TypeArguments);
        }

        public override int GetHashCode()
        {
            var ordinalComparer = StringComparer.Ordinal;
            
            var hc = new HashCode();
            hc.Add(Namespace, ordinalComparer);
            hc.Add(Container);
            hc.Add(Name, ordinalComparer);
            foreach (var tp in TypeArguments)
                hc.Add(tp);
            return hc.ToHashCode();
        }
    }
}