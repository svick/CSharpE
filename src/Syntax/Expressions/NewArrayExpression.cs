using System;
using System.Collections.Generic;
using System.Linq;
using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RoslynSyntaxFactory = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Roslyn = Microsoft.CodeAnalysis;

namespace CSharpE.Syntax
{
    public sealed class NewArrayExpression : Expression
    {
        private ArrayCreationExpressionSyntax syntax;

        internal NewArrayExpression(ArrayCreationExpressionSyntax syntax, SyntaxNode parent)
            : base(syntax)
        {
            this.syntax = syntax;
            Parent = parent;
        }

        public NewArrayExpression(TypeReference elementType, ArrayInitializer initializer = null)
            : this(elementType, new Expression[] { null }, initializer) { }

        public NewArrayExpression(TypeReference elementType, params Expression[] lengths)
            : this(elementType, lengths.AsEnumerable()) { }

        public NewArrayExpression(
            TypeReference elementType, IEnumerable<Expression> lengths, ArrayInitializer initializer = null)
        {
            this.ElementType = elementType;
            this.lengths = new ExpressionList(lengths, this);
            this.Initializer = initializer;
        }

        private TypeReference elementType;
        public TypeReference ElementType
        {
            get
            {
                if (elementType == null)
                    elementType = ArrayTypeReference.GetElementType(syntax.Type, this);

                return elementType;
            }
            set => SetNotNull(ref elementType, value);
        }

        private ExpressionList lengths;
        /// <remarks>
        /// Note that inferred lengths are represented as <c>null</c> expressions in the list.
        /// </remarks>
        public IList<Expression> Lengths
        {
            get
            {
                if (lengths == null)
                    lengths = new ExpressionList(syntax.Type.RankSpecifiers[0].Sizes, this);

                return lengths;
            }
            set => SetList(ref lengths, new ExpressionList(value, this));
        }

        private bool initializerSet;
        private ArrayInitializer initializer;
        public ArrayInitializer Initializer
        {
            get
            {
                if (!initializerSet)
                {
                    initializer = syntax.Initializer == null ? null : new ArrayInitializer(syntax.Initializer, this);
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
            GetAndResetChanged(ref changed);

            bool? thisChanged = false;

            var newType = elementType?.GetWrapped(ref thisChanged) ?? ArrayTypeReference.GetElementTypeSyntax(syntax.Type);
            var newLengths = lengths?.GetWrapped(ref thisChanged) ?? syntax.Type.RankSpecifiers[0].Sizes;
            var newInitializer = initializerSet ? initializer?.GetWrapped(ref thisChanged) : syntax.Initializer;

            if (syntax == null || thisChanged == true || ShouldAnnotate(syntax, changed))
            {
                var arrayType = ArrayTypeReference.AddArrayRankToType(
                    newType, RoslynSyntaxFactory.ArrayRankSpecifier(newLengths));

                syntax = RoslynSyntaxFactory.ArrayCreationExpression(arrayType, newInitializer);

                syntax = Annotate(syntax);

                SetChanged(ref changed);
            }

            return syntax;
        }

        private protected override void SetSyntaxImpl(Roslyn::SyntaxNode newSyntax)
        {
            syntax = (ArrayCreationExpressionSyntax)newSyntax;

            Set(ref elementType, null);
            SetList(ref lengths, null);
            Set(ref initializer, null);
        }

        private protected override SyntaxNode CloneImpl() => new NewArrayExpression(ElementType, Lengths, Initializer);

        public override IEnumerable<SyntaxNode> GetChildren() =>
            new SyntaxNode[] { ElementType }.Concat(Lengths).Concat(new[] { Initializer });

        public override void ReplaceExpressions<T>(Func<T, bool> filter, Func<T, Expression> projection)
        {
            base.ReplaceExpressions(filter, projection);

            Initializer?.ReplaceExpressions(filter, projection);
        }
    }
}