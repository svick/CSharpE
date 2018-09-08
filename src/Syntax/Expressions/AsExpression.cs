using System;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxKind;

namespace CSharpE.Syntax
{
    public sealed class AsExpression : BinaryExpression
    {
        private protected override SyntaxKind Kind => SyntaxKind.AsExpression;

        internal AsExpression(BinaryExpressionSyntax syntax, SyntaxNode parent)
            : base(syntax, parent) { }

        public AsExpression(Expression left, TypeReference right)
            : base(left, right) { }

        public new TypeReference Right
        {
            get => (TypeReference)base.Right;
            set => base.Right = value;
        }

        internal override SyntaxNode Clone() => new AsExpression(Left, Right);
    }
}