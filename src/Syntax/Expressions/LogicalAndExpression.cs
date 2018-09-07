using System;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxKind;

namespace CSharpE.Syntax
{
    public sealed class LogicalAndExpression : BinaryExpression
    {
        private protected override SyntaxKind Kind => SyntaxKind.LogicalAndExpression;

        internal LogicalAndExpression(BinaryExpressionSyntax syntax, SyntaxNode parent)
            : base(syntax, parent) { }

        public LogicalAndExpression(Expression left, Expression right)
            : base(left, right) { }

        internal override SyntaxNode Clone() => new LogicalAndExpression(Left, Right);
    }
}