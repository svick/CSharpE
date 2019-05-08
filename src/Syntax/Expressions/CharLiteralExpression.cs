using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RoslynSyntaxFactory = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Roslyn = Microsoft.CodeAnalysis;

namespace CSharpE.Syntax
{
    public sealed class CharLiteralExpression : LiteralExpression
    {
        internal CharLiteralExpression(LiteralExpressionSyntax syntax, SyntaxNode parent)
            : base(syntax)
        {
            Init(syntax);

            Parent = parent;
        }

        private void Init(LiteralExpressionSyntax syntax)
        {
            Syntax = syntax;
            Value = (char)syntax.Token.Value;
        }

        public CharLiteralExpression(char value) => Value = value;

        public new char Value { get; set; }

        protected override object ValueImpl => Value;

        private protected override LiteralExpressionSyntax GetWrappedLiteral(ref bool? changed)
        {
            GetAndResetChanged(ref changed);

            if (Syntax == null || Value != (char)Syntax.Token.Value || ShouldAnnotate(Syntax, changed))
            {
                Syntax = RoslynSyntaxFactory.LiteralExpression(
                    SyntaxKind.CharacterLiteralExpression, RoslynSyntaxFactory.Literal(Value));

                Syntax = Annotate(Syntax);

                SetChanged(ref changed);
            }

            return Syntax;
        }

        private protected override void SetSyntaxImpl(Roslyn::SyntaxNode newSyntax) =>
            Init((LiteralExpressionSyntax)newSyntax);

        private protected override SyntaxNode CloneImpl() => new CharLiteralExpression(Value);
    }
}