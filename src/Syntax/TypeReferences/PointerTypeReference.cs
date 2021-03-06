﻿using System;
using System.Text;
using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Roslyn = Microsoft.CodeAnalysis;
using RoslynSyntaxFactory = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace CSharpE.Syntax
{
    public sealed class PointerTypeReference : TypeReference
    {
        private PointerTypeSyntax syntax;

        internal PointerTypeReference(PointerTypeSyntax syntax, SyntaxNode parent)
            : base(syntax)
        {
            this.syntax = syntax;
            Parent = parent;
        }

        public PointerTypeReference(TypeReference elementType)
        {
            this.elementType = elementType;
        }

        internal PointerTypeReference(IPointerTypeSymbol pointerType)
            : this(FromRoslyn.TypeReference(pointerType.PointedAtType)) { }

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
            syntax = (PointerTypeSyntax)newSyntax;
        }

        private protected override SyntaxNode CloneImpl() => new PointerTypeReference(ElementType);

        private protected override TypeSyntax GetWrappedType(ref bool? changed)
        {
            GetAndResetChanged(ref changed, out var thisChanged);

            var newElementType = elementType?.GetWrapped(ref thisChanged) ?? syntax.ElementType;

            if (syntax == null || thisChanged == true || ShouldAnnotate(syntax, changed))
            {
                syntax = RoslynSyntaxFactory.PointerType(newElementType);

                syntax = Annotate(syntax);

                SetChanged(ref changed);
            }

            return syntax;
        }

        internal override StringBuilder ComputeFullName(StringBuilder stringBuilder) =>
            ElementType.ComputeFullName(stringBuilder).Append('*');

        public override bool Equals(TypeReference other)
        {
            if (ReferenceEquals(this, other)) return true;
            if (!(other is PointerTypeReference otherPointer)) return false;

            return Equals(ElementType, otherPointer.ElementType);
        }

        public override int GetHashCode() => HashCode.Combine(ElementType, nameof(PointerTypeReference));
    }
}
