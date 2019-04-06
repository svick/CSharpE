using System;
using System.Linq;
using System.Text;
using CSharpE.Syntax.Internals;
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
            this.elementType = elementType;
            Rank = rank;
        }

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
            set => elementType = value;
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
                !IsAnnotated(syntax))
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

        internal override SyntaxNode Parent { get; set; }

        private protected override void SetSyntaxImpl(Roslyn::SyntaxNode newSyntax)
        {
            Set(ref elementType, null);
            Init((ArrayTypeSyntax)newSyntax);
        }

        internal override SyntaxNode Clone() => new ArrayTypeReference(ElementType, Rank);

        internal override StringBuilder ComputeFullName(StringBuilder stringBuilder) =>
            throw new NotImplementedException();
    }
}
