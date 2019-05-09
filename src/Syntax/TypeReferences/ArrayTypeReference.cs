using System;
using System.Linq;
using System.Text;
using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Roslyn = Microsoft.CodeAnalysis;
using RoslynSyntaxFactory = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace CSharpE.Syntax
{
    public sealed class ArrayTypeReference : TypeReference
    {
        private ArrayTypeSyntax syntax;

        internal ArrayTypeReference(ArrayTypeSyntax syntax, SyntaxNode parent)
        {
            Init(syntax);
            Parent = parent;
        }

        private void Init(ArrayTypeSyntax syntax)
        {
            this.syntax = syntax;

            Rank = syntax.RankSpecifiers.First().Rank;
        }

        public ArrayTypeReference(TypeReference elementType)
            : this(elementType, 1) { }

        public ArrayTypeReference(TypeReference elementType, int rank)
        {
            ElementType = elementType;
            Rank = rank;
        }

        internal ArrayTypeReference(IArrayTypeSymbol arrayType)
            : this(FromRoslyn.TypeReference(arrayType.ElementType), arrayType.Rank) { }

        internal static TypeSyntax GetElementTypeSyntax(ArrayTypeSyntax syntax)
        {
            if (syntax.RankSpecifiers.Count >= 2)
                return syntax.WithRankSpecifiers(syntax.RankSpecifiers.RemoveAt(0));
            else
                return syntax.ElementType;
        }

        internal static TypeReference GetElementType(ArrayTypeSyntax syntax, SyntaxNode parent) => 
            FromRoslyn.TypeReference(GetElementTypeSyntax(syntax), parent);

        private TypeReference elementType;

        public TypeReference ElementType
        {
            get
            {
                if (elementType == null)
                    elementType = GetElementType(syntax, this);

                return elementType;
            }
            set => SetNotNull(ref elementType, value);
        }

        public int Rank { get; set; }

        internal static ArrayTypeSyntax AddArrayRankToType(TypeSyntax type, ArrayRankSpecifierSyntax rankSpecifier)
        {
            if (type is ArrayTypeSyntax arrayElementType)
            {
                return arrayElementType.WithRankSpecifiers(
                    arrayElementType.RankSpecifiers.Insert(0, rankSpecifier));
            }
            else
            {
                return RoslynSyntaxFactory.ArrayType(
                    type, RoslynSyntaxFactory.SingletonList(rankSpecifier));
            }
        }

        private protected override TypeSyntax GetWrappedType(ref bool? changed)
        {
            GetAndResetChanged(ref changed);

            bool? thisChanged = false;

            var newElementType = elementType?.GetWrapped(ref thisChanged);

            if (syntax == null || thisChanged == true || Rank != syntax.RankSpecifiers.First().Rank ||
                ShouldAnnotate(syntax, changed))
            {
                if (newElementType == null)
                    newElementType = ElementType.GetWrapped(ref thisChanged);

                var newRankSpecifier = RoslynSyntaxFactory.ArrayRankSpecifier(
                    RoslynSyntaxFactory.SeparatedList<ExpressionSyntax>(
                        Enumerable.Repeat(RoslynSyntaxFactory.OmittedArraySizeExpression(), Rank)));

                syntax = AddArrayRankToType(newElementType, newRankSpecifier);

                syntax = Annotate(syntax);

                SetChanged(ref changed);
            }

            return syntax;
        }

        private protected override void SetSyntaxImpl(Roslyn::SyntaxNode newSyntax)
        {
            Set(ref elementType, null);
            Init((ArrayTypeSyntax)newSyntax);
        }

        private protected override SyntaxNode CloneImpl() => new ArrayTypeReference(ElementType, Rank);

        internal override StringBuilder ComputeFullName(StringBuilder stringBuilder) =>
            ElementType.ComputeFullName(stringBuilder)
                .Append('[')
                .Append(',', Rank - 1)
                .Append(']');

        public override bool Equals(TypeReference other)
        {
            if (ReferenceEquals(this, other)) return true;
            if (!(other is ArrayTypeReference otherArray)) return false;

            return Equals(ElementType, otherArray.ElementType) && Rank == otherArray.Rank;
        }

        public override int GetHashCode() => HashCode.Combine(ElementType, Rank);
    }
}
