using System;
using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RoslynSyntaxFactory = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Roslyn = Microsoft.CodeAnalysis;

namespace CSharpE.Syntax
{
    public sealed class StackAllocExpression : Expression
    {
        private StackAllocArrayCreationExpressionSyntax syntax;

        internal StackAllocExpression(StackAllocArrayCreationExpressionSyntax syntax, SyntaxNode parent)
            : base(syntax)
        {
            Init(syntax);
            Parent = parent;
        }

        private void Init(StackAllocArrayCreationExpressionSyntax syntax)
        {
            this.syntax = syntax;

            // violating these conditions is allowed by Roslyn, but does not fit with CSharpE model for this type
            if (!(syntax.Type is ArrayTypeSyntax arrayType) || arrayType.RankSpecifiers.Count > 1 ||
                arrayType.RankSpecifiers[0].Sizes.Count > 1)
                throw new InvalidOperationException();
        }

        public StackAllocExpression(TypeReference elementType, ArrayInitializer initializer = null)
            : this(elementType, null, initializer) { }

        public StackAllocExpression(
            TypeReference elementType, Expression length, ArrayInitializer initializer = null)
        {
            ElementType = elementType;
            Length = length;
            Initializer = initializer;
        }

        private TypeSyntax GetSyntaxElementType() => ArrayTypeReference.GetElementTypeSyntax((ArrayTypeSyntax)syntax.Type);

        private TypeReference elementType;
        public TypeReference ElementType
        {
            get
            {
                if (elementType == null)
                    elementType = FromRoslyn.TypeReference(GetSyntaxElementType(), this);

                return elementType;
            }
            set => SetNotNull(ref elementType, value);
        }

        private ExpressionSyntax GetSyntaxLength()
        {
            var arrayType = (ArrayTypeSyntax)syntax.Type;
            return arrayType.RankSpecifiers[0].Sizes[0];
        }

        private bool lengthSet;
        private Expression length;
        public Expression Length
        {
            get
            {
                if (!lengthSet)
                {
                    length = FromRoslyn.Expression(GetSyntaxLength(), this);
                    lengthSet = true;
                }

                return length;
            }
            set
            {
                Set(ref length, value);
                lengthSet = true;
            }
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

            var newType = elementType?.GetWrapped(ref thisChanged) ?? GetSyntaxElementType();
            var newLength = lengthSet ? length?.GetWrapped(ref thisChanged) : GetSyntaxLength();
            var newInitializer = initializerSet ? initializer?.GetWrapped(ref thisChanged) : syntax.Initializer;

            if (syntax == null || thisChanged == true || ShouldAnnotate(syntax, changed))
            {
                if (newLength == null)
                    newLength = RoslynSyntaxFactory.OmittedArraySizeExpression();

                var arrayType = ArrayTypeReference.AddArrayRankToType(
                    newType,
                    RoslynSyntaxFactory.ArrayRankSpecifier(RoslynSyntaxFactory.SingletonSeparatedList(newLength)));

                syntax = RoslynSyntaxFactory.StackAllocArrayCreationExpression(arrayType, newInitializer);

                syntax = Annotate(syntax);

                SetChanged(ref changed);
            }

            return syntax;
        }

        private protected override void SetSyntaxImpl(Roslyn::SyntaxNode newSyntax)
        {
            syntax = (StackAllocArrayCreationExpressionSyntax)newSyntax;

            Set(ref elementType, null);
            Set(ref length, null);
            Set(ref initializer, null);
        }

        private protected override SyntaxNode CloneImpl() => new StackAllocExpression(ElementType, Length, Initializer);

        public override void ReplaceExpressions<T>(Func<T, bool> filter, Func<T, Expression> projection)
        {
            base.ReplaceExpressions(filter, projection);

            Initializer?.ReplaceExpressions(filter, projection);
        }
    }
}