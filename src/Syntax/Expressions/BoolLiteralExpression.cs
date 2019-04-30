using System;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxKind;
using RoslynSyntaxFactory = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Roslyn = Microsoft.CodeAnalysis;

namespace CSharpE.Syntax
{
    public sealed class BoolLiteralExpression : LiteralExpression
    {
        internal BoolLiteralExpression(LiteralExpressionSyntax syntax, SyntaxNode parent)
        {
            Init(syntax);

            Parent = parent;
        }

        private void Init(LiteralExpressionSyntax syntax)
        {
            Syntax = syntax;
            Value = syntax.Token.Value as bool? ?? throw new ArgumentException(nameof(syntax));
        }

        public BoolLiteralExpression(bool value) => Value = value;

        public new bool Value { get; set; }

        protected override object ValueImpl => Value;

        private protected override LiteralExpressionSyntax GetWrappedLiteral(ref bool? changed)
        {
            GetAndResetChanged(ref changed);

            if (Syntax == null || Value != (bool)Syntax.Token.Value)
            {
                Syntax = RoslynSyntaxFactory.LiteralExpression(Value ? TrueLiteralExpression : FalseLiteralExpression);

                SetChanged(ref changed);
            }

            return Syntax;
        }

        private protected override void SetSyntaxImpl(Roslyn::SyntaxNode newSyntax) =>
            Init((LiteralExpressionSyntax) newSyntax);

        private protected override SyntaxNode CloneImpl() => new BoolLiteralExpression(Value);
    }
}