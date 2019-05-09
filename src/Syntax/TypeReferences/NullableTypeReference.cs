using System;
using System.Text;
using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Roslyn = Microsoft.CodeAnalysis;
using RoslynSyntaxFactory = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace CSharpE.Syntax
{
    public sealed class NullableTypeReference : TypeReference
    {
        private NullableTypeSyntax syntax;

        internal NullableTypeReference(NullableTypeSyntax syntax, SyntaxNode parent)
            : base(syntax)
        {
            this.syntax = syntax;
            Parent = parent;
        }

        public NullableTypeReference(TypeReference elementType)
        {
            this.elementType = elementType;
        }

        private TypeReference elementType;
        public TypeReference ElementType
        {
            get
            {
                if (elementType == null)
                {
                    elementType = FromRoslyn.TypeReference(syntax.ElementType, this);
                }

                return elementType;
            }
            set => elementType = value;
        }

        private protected override void SetSyntaxImpl(Roslyn::SyntaxNode newSyntax)
        {
            elementType = null;
            syntax = (NullableTypeSyntax)newSyntax;
        }

        private protected override SyntaxNode CloneImpl() => new NullableTypeReference(ElementType);

        private protected override TypeSyntax GetWrappedType(ref bool? changed)
        {
            GetAndResetChanged(ref changed);

            bool? thisChanged = false;

            var newElementType = elementType?.GetWrapped(ref thisChanged) ?? syntax.ElementType;

            if (syntax == null || thisChanged == true || ShouldAnnotate(syntax, changed))
            {
                syntax = RoslynSyntaxFactory.NullableType(newElementType);

                syntax = Annotate(syntax);

                SetChanged(ref changed);
            }

            return syntax;
        }

        internal override StringBuilder ComputeFullName(StringBuilder stringBuilder) =>
            ElementType.ComputeFullName(stringBuilder).Append('?');

        public override bool Equals(TypeReference other)
        {
            if (ReferenceEquals(this, other)) return true;
            if (!(other is NullableTypeReference otherNullable)) return false;

            return Equals(ElementType, otherNullable.ElementType);
        }

        public override int GetHashCode() => HashCode.Combine(ElementType, nameof(NullableTypeReference));
    }
}
