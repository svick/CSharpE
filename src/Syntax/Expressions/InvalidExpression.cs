using Microsoft.CodeAnalysis.CSharp.Syntax;
using Roslyn = Microsoft.CodeAnalysis;

namespace CSharpE.Syntax
{
    public sealed class InvalidExpression : Expression
    {
        private ExpressionSyntax syntax;

        internal InvalidExpression(ExpressionSyntax expressionSyntax, SyntaxNode parent)
            : base(expressionSyntax)
        {
            syntax = expressionSyntax;
            Parent = parent;
        }

        private protected override void SetSyntaxImpl(Roslyn::SyntaxNode newSyntax) =>
            syntax = (ExpressionSyntax)newSyntax;

        private protected override SyntaxNode CloneImpl() => new InvalidExpression(syntax, null);

        private protected override ExpressionSyntax GetWrappedExpression(ref bool? changed) => syntax;
    }
}
