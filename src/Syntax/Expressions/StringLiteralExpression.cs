using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RoslynSyntaxFactory = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Roslyn = Microsoft.CodeAnalysis;

namespace CSharpE.Syntax
{
    public sealed class StringLiteralExpression : LiteralExpression
    {
        internal StringLiteralExpression(LiteralExpressionSyntax syntax, SyntaxNode parent)
        {
            Init(syntax);

            Parent = parent;
        }

        private void Init(LiteralExpressionSyntax syntax)
        {
            Syntax = syntax;
            Value = (string)syntax.Token.Value;
        }

        public StringLiteralExpression(string value) => Value = value;

        public new string Value { get; set; }

        protected override object ValueImpl => Value;

        private protected override LiteralExpressionSyntax GetWrappedLiteral(ref bool? changed)
        {
            GetAndResetChanged(ref changed);

            if (Syntax == null || Value != (string)Syntax.Token.Value)
            {
                Syntax = RoslynSyntaxFactory.LiteralExpression(
                    SyntaxKind.StringLiteralExpression, RoslynSyntaxFactory.Literal(Value));

                SetChanged(ref changed);
            }

            return Syntax;
        }

        private protected override void SetSyntaxImpl(Roslyn::SyntaxNode newSyntax) =>
            Init((LiteralExpressionSyntax)newSyntax);

        private protected override SyntaxNode CloneImpl() => new StringLiteralExpression(Value);
    }
}