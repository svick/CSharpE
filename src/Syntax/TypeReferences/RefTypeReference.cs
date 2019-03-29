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

        internal override SyntaxNode Parent { get; set; }

        private protected override void SetSyntaxImpl(Roslyn::SyntaxNode newSyntax)
        {
            elementType = null;
            syntax = (RefTypeSyntax)newSyntax;
        }

        internal override SyntaxNode Clone() => new RefTypeReference(ElementType);

        private protected override TypeSyntax GetWrappedType(ref bool? changed)
        {
            GetAndResetChanged(ref changed);

            bool? thisChanged = false;

            var newElementType = elementType?.GetWrapped(ref thisChanged) ?? syntax.Type;

            if (syntax == null || thisChanged == true || !IsAnnotated(syntax))
            {
                syntax = RoslynSyntaxFactory.RefType(newElementType);

                syntax = Annotate(syntax);

                SetChanged(ref changed);
            }

            return syntax;
        }

        internal override StringBuilder ComputeFullName(StringBuilder stringBuilder) =>
            throw new NotImplementedException();
    }
}