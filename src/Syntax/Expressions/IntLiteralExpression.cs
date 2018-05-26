using System;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxKind;
using CSharpSyntaxFactory = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Roslyn = Microsoft.CodeAnalysis;

namespace CSharpE.Syntax
{
    public sealed class IntLiteralExpression : LiteralExpression
    {
        internal IntLiteralExpression(LiteralExpressionSyntax syntax, SyntaxNode parent)
        {
            Init(syntax ?? throw new ArgumentNullException(nameof(syntax)));

            Parent = parent;
        }

        private void Init(LiteralExpressionSyntax syntax)
        {
            Syntax = syntax;
            Value = syntax.Token.Value as int? ?? throw new ArgumentException(nameof(syntax));
        }

        public IntLiteralExpression(int value) => Value = value;

        public new int Value { get; set; }

        protected override object ValueImpl => Value;

        internal override ExpressionSyntax GetWrapped()
        {
            if (Syntax == null || Value != (int)Syntax.Token.Value)
            {
                Syntax = CSharpSyntaxFactory.LiteralExpression(
                    NumericLiteralExpression, CSharpSyntaxFactory.Literal(Value));
            }

            return Syntax;
        }

        protected override void SetSyntaxImpl(Roslyn::SyntaxNode newSyntax) => Init((LiteralExpressionSyntax)newSyntax);
    }
}