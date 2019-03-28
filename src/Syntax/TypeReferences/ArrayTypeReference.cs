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

        private TypeReference elementType;
        public TypeReference ElementType
        {
            get
            {
                if (elementType == null)
                {
                    if (syntax.RankSpecifiers.Count >= 2)
                        elementType = new ArrayTypeReference(
                            syntax.WithRankSpecifiers(syntax.RankSpecifiers.RemoveAt(0)), this);
                    else
                        elementType = FromRoslyn.TypeReference(syntax.ElementType, this);
                }

                return elementType;
            }
            set => elementType = value;
        }

        public int Rank { get; set; }

        internal override SyntaxNode Parent { get; set; }

        private protected override void SetSyntaxImpl(Roslyn::SyntaxNode newSyntax)
        {
            elementType = null;
            Init((ArrayTypeSyntax)newSyntax);
        }

        internal override SyntaxNode Clone() => new ArrayTypeReference(ElementType, Rank);

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

                if (newElementType is ArrayTypeSyntax arrayElementType)
                {
                    syntax = arrayElementType.WithRankSpecifiers(
                        arrayElementType.RankSpecifiers.Insert(0, newRankSpecifier));
                }
                else
                {
                    syntax = RoslynSyntaxFactory.ArrayType(
                        newElementType, RoslynSyntaxFactory.SingletonList(newRankSpecifier));
                }

                syntax = Annotate(syntax);

                SetChanged(ref changed);
            }

            return syntax;
        }

        internal override StringBuilder ComputeFullName(StringBuilder stringBuilder) =>
            throw new NotImplementedException();
    }
}
