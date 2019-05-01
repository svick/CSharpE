using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Roslyn = Microsoft.CodeAnalysis;
using RoslynSyntaxFactory = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace CSharpE.Syntax
{
    public sealed class TupleTypeReference : TypeReference
    {
        private TupleTypeSyntax syntax;

        internal TupleTypeReference(TupleTypeSyntax syntax, SyntaxNode parent)
        {
            this.syntax = syntax;
            Parent = parent;
        }

        public TupleTypeReference(params TypeReference[] elementTypes) : this(elementTypes.AsEnumerable()) { }

        public TupleTypeReference(IEnumerable<TypeReference> elementTypes)
            : this(elementTypes.Select(t => new TupleElement(t))) { }

        public TupleTypeReference(params TupleElement[] elements) : this(elements.AsEnumerable()) { }

        public TupleTypeReference(IEnumerable<TupleElement> elements)
        {
            this.elements = new SeparatedSyntaxList<TupleElement, TupleElementSyntax>(elements, this);
        }

        private SeparatedSyntaxList<TupleElement, TupleElementSyntax> elements;
        public IList<TupleElement> Elements
        {
            get
            {
                if (elements == null)
                    elements = new SeparatedSyntaxList<TupleElement, TupleElementSyntax>(syntax.Elements, this);

                return elements;
            }
            set => SetList(ref elements, new SeparatedSyntaxList<TupleElement, TupleElementSyntax>(value, this));
        }

        private protected override void SetSyntaxImpl(Roslyn::SyntaxNode newSyntax)
        {
            elements = null;
            syntax = (TupleTypeSyntax)newSyntax;
        }

        private protected override SyntaxNode CloneImpl() => new TupleTypeReference(Elements);

        private protected override TypeSyntax GetWrappedType(ref bool? changed)
        {
            GetAndResetChanged(ref changed);

            bool? thisChanged = false;

            var newElements = elements?.GetWrapped(ref thisChanged) ?? syntax.Elements;

            if (syntax == null || thisChanged == true || !IsAnnotated(syntax))
            {
                syntax = RoslynSyntaxFactory.TupleType(newElements);

                syntax = Annotate(syntax);

                SetChanged(ref changed);
            }

            return syntax;
        }

        public override IEnumerable<SyntaxNode> GetChildren() => Elements;

        internal override StringBuilder ComputeFullName(StringBuilder stringBuilder)
        {
            stringBuilder.Append('(');

            bool first = true;

            foreach (var element in Elements)
            {
                if (!first)
                    stringBuilder.Append(", ");
                first = false;

                element.ComputeFullName(stringBuilder);
            }

            stringBuilder.Append(')');

            return stringBuilder;
        }

        public override bool Equals(TypeReference other)
        {
            if (ReferenceEquals(this, other)) return true;
            if (!(other is TupleTypeReference otherTuple)) return false;

            return Elements.SequenceEqual(otherTuple.Elements);
        }

        public override int GetHashCode()
        {
            var hc = new HashCode();

            foreach (var element in Elements)
            {
                hc.Add(element);
            }

            return hc.ToHashCode();
        }
    }
}
