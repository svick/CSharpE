using System;
using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RoslynSyntaxFactory = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Roslyn = Microsoft.CodeAnalysis;

#nullable enable

namespace CSharpE.Syntax
{
    public sealed class StackAllocExpression : Expression
    {
        private Union<StackAllocArrayCreationExpressionSyntax, ImplicitStackAllocArrayCreationExpressionSyntax> syntax;

        internal StackAllocExpression(ExpressionSyntax syntax, SyntaxNode parent)
            : base(syntax)
        {
            Init(syntax);
            Parent = parent;
        }

        private void Init(ExpressionSyntax syntax)
        {
            // violating these conditions is allowed by Roslyn, but it's not actually legal
            if (syntax is StackAllocArrayCreationExpressionSyntax explicitSyntax &&
                    (explicitSyntax.Type is not ArrayTypeSyntax arrayType || arrayType.RankSpecifiers.Count > 1 || arrayType.RankSpecifiers[0].Sizes.Count > 1))
                throw new InvalidOperationException();

            this.syntax = Union<StackAllocArrayCreationExpressionSyntax, ImplicitStackAllocArrayCreationExpressionSyntax>.FromEither(syntax);
        }

        public StackAllocExpression(ArrayInitializer initializer)
            : this(null, null, initializer) { }

        public StackAllocExpression(TypeReference elementType, ArrayInitializer? initializer = null)
            : this(elementType, null, initializer) { }

        public StackAllocExpression(
            TypeReference? elementType, Expression? length, ArrayInitializer? initializer = null)
        {
            ElementType = elementType;
            Length = length;
            Initializer = initializer;
        }

        private TypeSyntax? GetSyntaxElementType() =>
            syntax.SwitchN(explicitSyntax => ((ArrayTypeSyntax)explicitSyntax.Type).ElementType, implicitSyntax => null);

        private bool elementTypeSet;
        private TypeReference? elementType;
        public TypeReference? ElementType
        {
            get
            {
                if (!elementTypeSet)
                {
                    elementType = FromRoslyn.TypeReference(GetSyntaxElementType(), this);
                    elementTypeSet = true;
                }

                return elementType;
            }
            set
            {
                if (value is ArrayTypeReference)
                    throw new ArgumentException("ElementType can't be array type.");

                Set(ref elementType, value);
                elementTypeSet = true;
            }
        }

        private ExpressionSyntax? GetSyntaxLength() =>
            syntax.SwitchN(explicitSyntax => ((ArrayTypeSyntax)explicitSyntax.Type).RankSpecifiers[0].Sizes[0], implicitSyntax => null);

        private bool lengthSet;
        private Expression? length;
        public Expression? Length
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

            var newType = elementType?.GetWrapped(ref thisChanged) ?? GetSyntaxElementType();
            var newLength = lengthSet ? length?.GetWrapped(ref thisChanged) : GetSyntaxLength();
            var newInitializer = initializerSet ? initializer?.GetWrapped(ref thisChanged) : GetSyntaxInitializer();

            var expressionSyntax = syntax.GetBase<ExpressionSyntax>();
            if (expressionSyntax == null || thisChanged == true || ShouldAnnotate(expressionSyntax, changed))
            {
                newLength ??= RoslynSyntaxFactory.OmittedArraySizeExpression();

                if (newType is null)
                {
                    if (newInitializer is null)
                        throw new InvalidOperationException("StackAllocExpression with null ElementType requires Initializer to be set.");
                    if (newLength is not OmittedArraySizeExpressionSyntax)
                        throw new InvalidOperationException("StackAllocExpression with null ElementType requires Length to be null.");

                    var implicitSyntax = RoslynSyntaxFactory.ImplicitStackAllocArrayCreationExpression(newInitializer);

                    syntax = Annotate(implicitSyntax);
                }
                else
                {
                    var arrayType = RoslynSyntaxFactory.ArrayType(newType)
                        .AddRankSpecifiers(RoslynSyntaxFactory.ArrayRankSpecifier().AddSizes(newLength));

                    var explicitSyntax = RoslynSyntaxFactory.StackAllocArrayCreationExpression(arrayType, newInitializer);

                    syntax = Annotate(explicitSyntax);
                }

                SetChanged(ref changed);
            }

            return syntax.GetBase<ExpressionSyntax>()!;
        }

        private protected override void SetSyntaxImpl(Roslyn::SyntaxNode newSyntax)
        {
            syntax = (StackAllocArrayCreationExpressionSyntax)newSyntax;

            Set(ref elementType, null);
            elementTypeSet = false;
            Set(ref length, null);
            lengthSet = false;
            Set(ref initializer, null);
            initializerSet = false;
        }

        private protected override SyntaxNode CloneImpl() => new StackAllocExpression(ElementType, Length, Initializer);

        public override void ReplaceExpressions<T>(Func<T, bool> filter, Func<T, Expression> projection)
        {
            base.ReplaceExpressions(filter, projection);

            Initializer?.ReplaceExpressions(filter, projection);
        }
    }
}