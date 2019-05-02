using System;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RoslynSyntaxFactory = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Roslyn = Microsoft.CodeAnalysis;

namespace CSharpE.Syntax
{
    public sealed class ImplicitStackAllocExpression : Expression
    {
        private ImplicitStackAllocArrayCreationExpressionSyntax syntax;

        internal ImplicitStackAllocExpression(ImplicitStackAllocArrayCreationExpressionSyntax syntax, SyntaxNode parent)
        {
            this.syntax = syntax;
            Parent = parent;
        }

        public ImplicitStackAllocExpression(ArrayInitializer initializer) => Initializer = initializer;

        private bool initializerSet;
        private ArrayInitializer initializer;
        public ArrayInitializer Initializer
        {
            get
            {
                if (!initializerSet)
                {
                    initializer = new ArrayInitializer(syntax.Initializer, this);
                    initializerSet = true;
                }

                return initializer;
            }
            set
            {
                SetNotNull(ref initializer, value);
                initializerSet = true;
            }
        }

        private protected override ExpressionSyntax GetWrappedExpression(ref bool? changed)
        {
            GetAndResetChanged(ref changed);

            bool? thisChanged = false;

            var newInitializer = initializerSet ? initializer.GetWrapped(ref thisChanged) : syntax.Initializer;

            if (syntax == null || thisChanged == true)
            {
                syntax = RoslynSyntaxFactory.ImplicitStackAllocArrayCreationExpression(newInitializer);

                SetChanged(ref changed);
            }

            return syntax;
        }

        private protected override void SetSyntaxImpl(Roslyn::SyntaxNode newSyntax)
        {
            syntax = (ImplicitStackAllocArrayCreationExpressionSyntax)newSyntax;

            Set(ref initializer, null);
        }

        private protected override SyntaxNode CloneImpl() => new ImplicitStackAllocExpression(Initializer);

        public override void ReplaceExpressions<T>(Func<T, bool> filter, Func<T, Expression> projection) =>
            Initializer.ReplaceExpressions(filter, projection);
    }
}