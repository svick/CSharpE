using System;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxKind;

namespace CSharpE.Syntax
{
    public sealed class AssignmentExpression : BinaryExpression
    {
        private protected override SyntaxKind Kind => SyntaxKind.SimpleAssignmentExpression;

        private protected override bool IsAssignment => true;

        internal AssignmentExpression(AssignmentExpressionSyntax syntax, SyntaxNode parent)
            : base(syntax, parent) { }

        public AssignmentExpression(Expression left, Expression right)
            : base(left, right) { }

        internal override SyntaxNode Clone() => new AssignmentExpression(Left, Right);
    }
}