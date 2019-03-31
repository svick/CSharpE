using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

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

    public sealed class AddExpression : BinaryExpression
    {
        internal AddExpression(BinaryExpressionSyntax syntax, SyntaxNode parent)
            : base(syntax, parent) { }

        public AddExpression(Expression left, Expression right)
            : base(left, right) { }

        internal override SyntaxNode Clone() => new AddExpression(Left, Right);
    }

    public sealed class SubtractExpression : BinaryExpression
    {
        internal SubtractExpression(BinaryExpressionSyntax syntax, SyntaxNode parent)
            : base(syntax, parent) { }

        public SubtractExpression(Expression left, Expression right)
            : base(left, right) { }

        internal override SyntaxNode Clone() => new SubtractExpression(Left, Right);
    }

    public sealed class MultiplyExpression : BinaryExpression
    {
        internal MultiplyExpression(BinaryExpressionSyntax syntax, SyntaxNode parent)
            : base(syntax, parent) { }

        public MultiplyExpression(Expression left, Expression right)
            : base(left, right) { }

        internal override SyntaxNode Clone() => new MultiplyExpression(Left, Right);
    }

    public sealed class DivideExpression : BinaryExpression
    {
        internal DivideExpression(BinaryExpressionSyntax syntax, SyntaxNode parent)
            : base(syntax, parent) { }

        public DivideExpression(Expression left, Expression right)
            : base(left, right) { }

        internal override SyntaxNode Clone() => new DivideExpression(Left, Right);
    }

    public sealed class ModuloExpression : BinaryExpression
    {
        internal ModuloExpression(BinaryExpressionSyntax syntax, SyntaxNode parent)
            : base(syntax, parent) { }

        public ModuloExpression(Expression left, Expression right)
            : base(left, right) { }

        internal override SyntaxNode Clone() => new ModuloExpression(Left, Right);
    }

    public sealed class LeftShiftExpression : BinaryExpression
    {
        internal LeftShiftExpression(BinaryExpressionSyntax syntax, SyntaxNode parent)
            : base(syntax, parent) { }

        public LeftShiftExpression(Expression left, Expression right)
            : base(left, right) { }

        internal override SyntaxNode Clone() => new LeftShiftExpression(Left, Right);
    }

    public sealed class RightShiftExpression : BinaryExpression
    {
        internal RightShiftExpression(BinaryExpressionSyntax syntax, SyntaxNode parent)
            : base(syntax, parent) { }

        public RightShiftExpression(Expression left, Expression right)
            : base(left, right) { }

        internal override SyntaxNode Clone() => new RightShiftExpression(Left, Right);
    }

    public sealed class LogicalOrExpression : BinaryExpression
    {
        internal LogicalOrExpression(BinaryExpressionSyntax syntax, SyntaxNode parent)
            : base(syntax, parent) { }

        public LogicalOrExpression(Expression left, Expression right)
            : base(left, right) { }

        internal override SyntaxNode Clone() => new LogicalOrExpression(Left, Right);
    }

    public sealed class LogicalAndExpression : BinaryExpression
    {
        internal LogicalAndExpression(BinaryExpressionSyntax syntax, SyntaxNode parent)
            : base(syntax, parent) { }

        public LogicalAndExpression(Expression left, Expression right)
            : base(left, right) { }

        internal override SyntaxNode Clone() => new LogicalAndExpression(Left, Right);
    }

    public sealed class BitwiseOrExpression : BinaryExpression
    {
        internal BitwiseOrExpression(BinaryExpressionSyntax syntax, SyntaxNode parent)
            : base(syntax, parent) { }

        public BitwiseOrExpression(Expression left, Expression right)
            : base(left, right) { }

        internal override SyntaxNode Clone() => new BitwiseOrExpression(Left, Right);
    }

    public sealed class BitwiseAndExpression : BinaryExpression
    {
        internal BitwiseAndExpression(BinaryExpressionSyntax syntax, SyntaxNode parent)
            : base(syntax, parent) { }

        public BitwiseAndExpression(Expression left, Expression right)
            : base(left, right) { }

        internal override SyntaxNode Clone() => new BitwiseAndExpression(Left, Right);
    }

    public sealed class ExclusiveOrExpression : BinaryExpression
    {
        internal ExclusiveOrExpression(BinaryExpressionSyntax syntax, SyntaxNode parent)
            : base(syntax, parent) { }

        public ExclusiveOrExpression(Expression left, Expression right)
            : base(left, right) { }

        internal override SyntaxNode Clone() => new ExclusiveOrExpression(Left, Right);
    }

    public sealed class EqualsExpression : BinaryExpression
    {
        internal EqualsExpression(BinaryExpressionSyntax syntax, SyntaxNode parent)
            : base(syntax, parent) { }

        public EqualsExpression(Expression left, Expression right)
            : base(left, right) { }

        internal override SyntaxNode Clone() => new EqualsExpression(Left, Right);
    }

    public sealed class NotEqualsExpression : BinaryExpression
    {
        internal NotEqualsExpression(BinaryExpressionSyntax syntax, SyntaxNode parent)
            : base(syntax, parent) { }

        public NotEqualsExpression(Expression left, Expression right)
            : base(left, right) { }

        internal override SyntaxNode Clone() => new NotEqualsExpression(Left, Right);
    }

    public sealed class LessThanExpression : BinaryExpression
    {
        internal LessThanExpression(BinaryExpressionSyntax syntax, SyntaxNode parent)
            : base(syntax, parent) { }

        public LessThanExpression(Expression left, Expression right)
            : base(left, right) { }

        internal override SyntaxNode Clone() => new LessThanExpression(Left, Right);
    }

    public sealed class LessThanOrEqualExpression : BinaryExpression
    {
        internal LessThanOrEqualExpression(BinaryExpressionSyntax syntax, SyntaxNode parent)
            : base(syntax, parent) { }

        public LessThanOrEqualExpression(Expression left, Expression right)
            : base(left, right) { }

        internal override SyntaxNode Clone() => new LessThanOrEqualExpression(Left, Right);
    }

    public sealed class GreaterThanExpression : BinaryExpression
    {
        internal GreaterThanExpression(BinaryExpressionSyntax syntax, SyntaxNode parent)
            : base(syntax, parent) { }

        public GreaterThanExpression(Expression left, Expression right)
            : base(left, right) { }

        internal override SyntaxNode Clone() => new GreaterThanExpression(Left, Right);
    }

    public sealed class GreaterThanOrEqualExpression : BinaryExpression
    {
        internal GreaterThanOrEqualExpression(BinaryExpressionSyntax syntax, SyntaxNode parent)
            : base(syntax, parent) { }

        public GreaterThanOrEqualExpression(Expression left, Expression right)
            : base(left, right) { }

        internal override SyntaxNode Clone() => new GreaterThanOrEqualExpression(Left, Right);
    }

    public sealed class IsExpression : BinaryExpression
    {
        internal IsExpression(BinaryExpressionSyntax syntax, SyntaxNode parent)
            : base(syntax, parent) { }

        public IsExpression(Expression left, TypeReference right)
            : base(left, right) { }

        public new TypeReference Right
        {
            get => (TypeReference)base.Right;
            set => base.Right = value;
        }

        internal override SyntaxNode Clone() => new IsExpression(Left, Right);
    }

    public sealed class AsExpression : BinaryExpression
    {
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

    public sealed class CoalesceExpression : BinaryExpression
    {
        internal CoalesceExpression(BinaryExpressionSyntax syntax, SyntaxNode parent)
            : base(syntax, parent) { }

        public CoalesceExpression(Expression left, Expression right)
            : base(left, right) { }

        internal override SyntaxNode Clone() => new CoalesceExpression(Left, Right);
    }
}