using System;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RoslynSyntaxFactory = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Roslyn = Microsoft.CodeAnalysis;

namespace CSharpE.Syntax
{
    public sealed class ImplicitNewArrayExpression : Expression
    {
        private ImplicitArrayCreationExpressionSyntax syntax;

        internal ImplicitNewArrayExpression(ImplicitArrayCreationExpressionSyntax syntax, SyntaxNode parent)
            : base(syntax)
        {
            Init(syntax);
            Parent = parent;
        }

        private void Init(ImplicitArrayCreationExpressionSyntax syntax)
        {
            this.syntax = syntax;
            Rank = GetSyntaxRank();
        }

        public ImplicitNewArrayExpression(ArrayInitializer initializer) : this(1, initializer) { }

        public ImplicitNewArrayExpression(int rank, ArrayInitializer initializer)
        {
            Rank = rank;
            Initializer = initializer;
        }

        private int GetSyntaxRank() => syntax.Commas.Count + 1;

        public int Rank { get; set; }

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

            var newInitializer = initializerSet ? initializer?.GetWrapped(ref thisChanged) : syntax.Initializer;

            if (syntax == null || thisChanged == true || Rank != GetSyntaxRank() || ShouldAnnotate(syntax, changed))
            {
                var commas = RoslynSyntaxFactory.TokenList(
                    Enumerable.Repeat(RoslynSyntaxFactory.Token(SyntaxKind.CommaToken), Rank - 1));

                syntax = RoslynSyntaxFactory.ImplicitArrayCreationExpression(commas, newInitializer);

                syntax = Annotate(syntax);

                SetChanged(ref changed);
            }

            return syntax;
        }

        private protected override void SetSyntaxImpl(Roslyn::SyntaxNode newSyntax)
        {
            Set(ref initializer, null);
            Init((ImplicitArrayCreationExpressionSyntax)newSyntax);
        }

        private protected override SyntaxNode CloneImpl() => new ImplicitNewArrayExpression(Rank, Initializer);

        public override void ReplaceExpressions<T>(Func<T, bool> filter, Func<T, Expression> projection) =>
            Initializer.ReplaceExpressions(filter, projection);
    }
}