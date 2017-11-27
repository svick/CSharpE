using System;
using System.Collections.Generic;
using System.Linq;
using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using CSharpSyntaxFactory = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace CSharpE.Syntax {
    public class NamedTypeReference : TypeReference
    {
        private TypeSyntax syntax;
        private SyntaxContext syntaxContext;

        // name used by syntax, computed lazily
        private string syntaxFullName;
        private string fullName;

        public string FullName
        {
            get
            {
                if (fullName == null && syntaxFullName == null)
                {
                    syntaxFullName = syntaxContext.GetFullName(syntax);
                    syntaxContext = default;
                }

                return fullName ?? syntaxFullName;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                    throw new ArgumentException(nameof(value));

                fullName = value;
            }
        }

        public string Name => FullName.Split('.').Last();

        private SeparatedSyntaxList<TypeReference, TypeSyntax> typeParameters;
        public IList<TypeReference> TypeParameters
        {
            get
            {
                if (typeParameters == null)
                {
                    typeParameters = syntax is GenericNameSyntax genericSyntax
                        ? new SeparatedSyntaxList<TypeReference, TypeSyntax>(genericSyntax.TypeArgumentList.Arguments)
                        : new SeparatedSyntaxList<TypeReference, TypeSyntax>();
                }

                return typeParameters;
            }
            set => typeParameters = new SeparatedSyntaxList<TypeReference, TypeSyntax>(value);
        }

        public NamedTypeReference(string fullName, TypeReference[] typeParameters)
            : this(fullName, typeParameters.AsEnumerable()) { }

        public NamedTypeReference(string fullName, IEnumerable<TypeReference> typeParameters)
        {
            if (string.IsNullOrEmpty(fullName))
                throw new ArgumentException(nameof(fullName));

            string StripGenericArity(string name)
            {
                var backtickIndex = name.IndexOf('`');

                if (backtickIndex == -1)
                    return name;

                return name.Substring(0, backtickIndex);
            }

            FullName = StripGenericArity(fullName);
            this.typeParameters =
                new SeparatedSyntaxList<TypeReference, TypeSyntax>(typeParameters ?? Array.Empty<TypeReference>());
        }

        public NamedTypeReference(Type type)
            : this(type.FullName, type.GenericTypeArguments.Select(a => (TypeReference)a)) { }

        internal NamedTypeReference(TypeSyntax syntax, SyntaxContext syntaxContext)
        {
            this.syntax = syntax;
            this.syntaxContext = syntaxContext;
        }

        public static implicit operator NamedTypeReference(Type type) => new NamedTypeReference(type);

        protected override TypeSyntax GetWrappedImpl(WrapperContext context)
        {
            var newTypeParameters = typeParameters?.GetWrapped(context);
            var oldTypeParameters = (syntax as GenericNameSyntax)?.TypeArgumentList.Arguments;

            if (syntax == null ||
                syntaxFullName != fullName || oldTypeParameters != newTypeParameters)
            {
                if (newTypeParameters?.Any() == true)
                {
                    syntax = CSharpSyntaxFactory.GenericName(
                        CSharpSyntaxFactory.Identifier(Name), CSharpSyntaxFactory.TypeArgumentList(newTypeParameters.Value));
                }
                else
                {
                    syntax = (TypeSyntax)GetPredefinedType(fullName) ?? CSharpSyntaxFactory.IdentifierName(Name);
                }

                syntaxFullName = fullName;
                context = default;
            }

            return syntax;
        }

        private PredefinedTypeSyntax GetPredefinedType(string name)
        {
            var kind = GetPredefinedSyntaxKind(name);

            if (kind == SyntaxKind.None)
                return null;

            return CSharpSyntaxFactory.PredefinedType(CSharpSyntaxFactory.Token(kind));
        }

        private SyntaxKind GetPredefinedSyntaxKind(string name)
        {
            switch (name)
            {
                case "System.Boolean":
                    return SyntaxKind.BoolKeyword;
                case "System.Byte":
                    return SyntaxKind.ByteKeyword;
                case "System.SByte":
                    return SyntaxKind.SByteKeyword;
                case "System.Int32":
                    return SyntaxKind.IntKeyword;
                case "System.UInt32":
                    return SyntaxKind.UIntKeyword;
                case "System.Int16":
                    return SyntaxKind.ShortKeyword;
                case "System.UInt16":
                    return SyntaxKind.UShortKeyword;
                case "System.Int64":
                    return SyntaxKind.LongKeyword;
                case "System.UInt64":
                    return SyntaxKind.ULongKeyword;
                case "System.Single":
                    return SyntaxKind.FloatKeyword;
                case "System.Double":
                    return SyntaxKind.DoubleKeyword;
                case "System.Decimal":
                    return SyntaxKind.DecimalKeyword;
                case "System.String":
                    return SyntaxKind.StringKeyword;
                case "System.Char":
                    return SyntaxKind.CharKeyword;
                case "System.Object":
                    return SyntaxKind.ObjectKeyword;
                case "System.Void":
                    return SyntaxKind.VoidKeyword;
                default:
                    return SyntaxKind.None;
            }
        }

        public override string ToString() => FullName;
    }
}