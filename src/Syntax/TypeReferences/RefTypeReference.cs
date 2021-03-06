﻿using System;
using System.Text;
using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Roslyn = Microsoft.CodeAnalysis;
using RoslynSyntaxFactory = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace CSharpE.Syntax
{
    public sealed class RefTypeReference : TypeReference
    {
        private RefTypeSyntax syntax;

        internal RefTypeReference(RefTypeSyntax syntax, SyntaxNode parent)
            : base(syntax)
        {
            this.syntax = syntax;
            Parent = parent;
        }

        public RefTypeReference(TypeReference elementType)
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
                    elementType = FromRoslyn.TypeReference(syntax.Type, this);
                }

                return elementType;
            }
            set => elementType = value;
        }

        private protected override void SetSyntaxImpl(Roslyn::SyntaxNode newSyntax)
        {
            elementType = null;
            syntax = (RefTypeSyntax)newSyntax;
        }

        private protected override SyntaxNode CloneImpl() => new RefTypeReference(ElementType);

        private protected override TypeSyntax GetWrappedType(ref bool? changed)
        {
            GetAndResetChanged(ref changed, out var thisChanged);

            var newElementType = elementType?.GetWrapped(ref thisChanged) ?? syntax.Type;

            if (syntax == null || thisChanged == true || ShouldAnnotate(syntax, changed))
            {
                syntax = RoslynSyntaxFactory.RefType(newElementType);

                syntax = Annotate(syntax);

                SetChanged(ref changed);
            }

            return syntax;
        }

        internal override StringBuilder ComputeFullName(StringBuilder stringBuilder) =>
            ElementType.ComputeFullName(stringBuilder.Append("ref "));

        public override bool Equals(TypeReference other)
        {
            if (ReferenceEquals(this, other)) return true;
            if (!(other is RefTypeReference otherRef)) return false;

            return Equals(ElementType, otherRef.ElementType);
        }

        public override int GetHashCode() => HashCode.Combine(ElementType, nameof(RefTypeReference));
    }
}
