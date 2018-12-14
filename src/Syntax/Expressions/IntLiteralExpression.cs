using System;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxKind;
using RoslynSyntaxFactory = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
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

        private protected override LiteralExpressionSyntax GetWrappedLiteral(ref bool? changed)
        {
            GetAndResetChanged(ref changed);

            if (Syntax == null || Value != (int)Syntax.Token.Value)
            {
                Syntax = RoslynSyntaxFactory.LiteralExpression(
                    NumericLiteralExpression, RoslynSyntaxFactory.Literal(Value));

                SetChanged(ref changed);
            }

            return Syntax;
        }

        private protected override void SetSyntaxImpl(Roslyn::SyntaxNode newSyntax) => Init((LiteralExpressionSyntax)newSyntax);

        internal override SyntaxNode Clone() => new IntLiteralExpression(Value);
    }
}