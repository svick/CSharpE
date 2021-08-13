using System;
using System.Collections.Generic;
using System.Linq;
using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RoslynSyntaxFactory = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Roslyn = Microsoft.CodeAnalysis;

#nullable enable

namespace CSharpE.Syntax
{
    public sealed class NewArrayExpression : Expression
    {
        private Union<ArrayCreationExpressionSyntax, ImplicitArrayCreationExpressionSyntax> syntax;

        internal NewArrayExpression(ExpressionSyntax syntax, SyntaxNode parent)
            : base(syntax)
        {
            this.syntax = Union<ArrayCreationExpressionSyntax, ImplicitArrayCreationExpressionSyntax>.FromEither(syntax);
            Parent = parent;
        }

        public NewArrayExpression(TypeReference elementType, ArrayInitializer? initializer = null)
            : this(elementType, new Expression?[] { null }, initializer) { }

        public NewArrayExpression(TypeReference elementType, params Expression?[] lengths)
            : this(elementType, lengths.AsEnumerable()) { }

        public NewArrayExpression(ArrayInitializer initializer) : this(1, initializer) { }

        public NewArrayExpression(int rank, ArrayInitializer initializer) : this(null, new Expression[rank], initializer) { }

        public NewArrayExpression(
            TypeReference? elementType, IEnumerable<Expression?> lengths, ArrayInitializer? initializer = null)
        {
            this.ElementType = elementType;
            this.lengths = new ExpressionList(lengths, this);
            this.Initializer = initializer;
        }

        private TypeSyntax? GetSyntaxType() =>
            syntax.SwitchN(explicitSyntax => ArrayTypeReference.GetElementTypeSyntax(explicitSyntax.Type), _ => null);

        private bool elementTypeSet;
        private TypeReference? elementType;
        public TypeReference? ElementType
        {
            get
            {
                if (!elementTypeSet)
                {
                    elementType = FromRoslyn.TypeReference(GetSyntaxType(), this);
                    elementTypeSet = true;
                }

                return elementType;
            }
            set => Set(ref elementType, value);
        }

        /// <remarks>
        /// Setting this property makes the corresponding lengths inferred.
        /// </remarks>
        public int Rank
        {
            get => Lengths.Count;
            set => Lengths = new Expression[value];
        }

        private ExpressionList? lengths;
        /// <remarks>
        /// Note that inferred lengths are represented as <c>null</c> expressions in the list.
        /// </remarks>
        public IList<Expression?> Lengths
        {
            get
            {
                if (lengths == null)
                {
                    lengths = syntax.Switch(
                        explicitSyntax => new ExpressionList(explicitSyntax.Type.RankSpecifiers[0].Sizes, this),
                        implicitSyntax => new ExpressionList(new Expression[implicitSyntax.Commas.Count + 1], this));
                }

                return lengths;
            }
            set => SetList(ref lengths, new ExpressionList(value, this));
        }

        private InitializerExpressionSyntax? GetSyntaxInitializer() =>
            syntax.Switch(explicitSyntax => explicitSyntax.Initializer, implicitSyntax => implicitSyntax.Initializer);

        private bool initializerSet;
        private ArrayInitializer? initializer;
        public ArrayInitializer? Initializer
        {
            get
            {
                if (!initializerSet)
                {
                    var syntaxInitializer = GetSyntaxInitializer();
                    initializer = syntaxInitializer is null ? null : new ArrayInitializer(syntaxInitializer, this);
                    initializerSet = true;
                }

                return initializer;
            }
            set
            {
                Set(ref initializer, value);
                initializerSet = true;
            }
        }

        private protected override ExpressionSyntax GetWrappedExpression(ref bool? changed)
        {
            GetAndResetChanged(ref changed, out var thisChanged);

            var newType = elementType?.GetWrapped(ref thisChanged) ?? GetSyntaxType();
            var newLengths = lengths?.GetWrapped(ref thisChanged);
            var newInitializer = initializerSet ? initializer?.GetWrapped(ref thisChanged) : GetSyntaxInitializer();

            var expressionSyntax = syntax.GetBase<ExpressionSyntax>();
            if (expressionSyntax == null || thisChanged == true || ShouldAnnotate(expressionSyntax, changed))
            {
                if (newLengths is null)
                {
                    // PERF
                    _ = Lengths;
                    newLengths = lengths!.GetWrapped(ref thisChanged);
                }

                if (newType is null)
                {
                    if (newInitializer is null)
                        throw new InvalidOperationException("NewArrayExpression with null ElementType requires Initializer to be set.");
                    if (newLengths.Value.Any(l => l is not OmittedArraySizeExpressionSyntax))
                        throw new InvalidOperationException("NewArrayExpression with null ElementType requires all Lengths to be null.");

                    var commas = RoslynSyntaxFactory.TokenList(
                        Enumerable.Repeat(RoslynSyntaxFactory.Token(SyntaxKind.CommaToken), newLengths.Value.Count - 1));

                    var implicitSyntax = RoslynSyntaxFactory.ImplicitArrayCreationExpression(commas, newInitializer);

                    syntax = Annotate(implicitSyntax);
                }
                else
                {
                    var arrayType = ArrayTypeReference.AddArrayRankToType(
                        newType, RoslynSyntaxFactory.ArrayRankSpecifier(newLengths.Value));

                    var explicitSyntax = RoslynSyntaxFactory.ArrayCreationExpression(arrayType, newInitializer);

                    syntax = Annotate(explicitSyntax);
                }

                SetChanged(ref changed);
            }

            return syntax.GetBase<ExpressionSyntax>()!;
        }

        private protected override void SetSyntaxImpl(Roslyn::SyntaxNode newSyntax)
        {
            syntax = (ArrayCreationExpressionSyntax)newSyntax;

            Set(ref elementType, null);
            elementTypeSet = false;
            SetList(ref lengths, null);
            Set(ref initializer, null);
            initializerSet = false;
        }

        private protected override SyntaxNode CloneImpl() => new NewArrayExpression(ElementType, Lengths, Initializer);

        public override void ReplaceExpressions<T>(Func<T, bool> filter, Func<T, Expression> projection)
        {
            base.ReplaceExpressions(filter, projection);

            Initializer?.ReplaceExpressions(filter, projection);
        }
    }
}