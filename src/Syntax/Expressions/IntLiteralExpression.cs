using System;
using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxKind;
using CSharpSyntaxFactory = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace CSharpE.Syntax
{
    public class IntLiteralExpression : LiteralExpression
    {
        internal IntLiteralExpression(LiteralExpressionSyntax syntax)
        {
            this.Syntax = syntax ?? throw new ArgumentNullException(nameof(syntax));
            Value = syntax.Token.Value as int? ?? throw new ArgumentException(nameof(syntax));
        }

        public IntLiteralExpression(int value) => Value = value;

        public new int Value { get; set; }

        protected override object ValueImpl => Value;

        internal override ExpressionSyntax GetWrapped(WrapperContext context)
        {
            if (Syntax == null || Value != (int)Syntax.Token.Value)
            {
                Syntax = CSharpSyntaxFactory.LiteralExpression(
                    NumericLiteralExpression, CSharpSyntaxFactory.Literal(Value));
            }

            return Syntax;
        }
    }
}