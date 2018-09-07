using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using CSharpSyntaxFactory = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Roslyn = Microsoft.CodeAnalysis;

namespace CSharpE.Syntax
{
    public sealed class NullExpression : LiteralExpression
    {
        internal NullExpression(LiteralExpressionSyntax syntax, SyntaxNode parent)
        {
            Init(syntax);

            Parent = parent;
        }

        private void Init(LiteralExpressionSyntax syntax) => Syntax = syntax;

        public NullExpression() { }

        protected override object ValueImpl => null;

        private protected override LiteralExpressionSyntax GetWrappedLiteral(ref bool? changed)
        {
            GetAndResetChanged(ref changed);

            if (Syntax == null)
            {
                Syntax = CSharpSyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression);

                SetChanged(ref changed);
            }

            return Syntax;
        }

        private protected override void SetSyntaxImpl(Roslyn::SyntaxNode newSyntax) =>
            Init((LiteralExpressionSyntax) newSyntax);

        internal override SyntaxNode Clone() => new NullExpression();
        
    }
}