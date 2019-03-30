using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

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