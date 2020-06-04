using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSharpE.Syntax
{
    public sealed class UnaryPlusExpression : UnaryExpression
    {
        internal UnaryPlusExpression(PrefixUnaryExpressionSyntax syntax, SyntaxNode parent)
            : base(syntax, parent) { }

        public UnaryPlusExpression(Expression operand) : base(operand) { }

        private protected override SyntaxNode CloneImpl() => new UnaryPlusExpression(Operand);
    }

    public sealed class UnaryMinusExpression : UnaryExpression
    {
        internal UnaryMinusExpression(PrefixUnaryExpressionSyntax syntax, SyntaxNode parent)
            : base(syntax, parent) { }

        public UnaryMinusExpression(Expression operand) : base(operand) { }

        private protected override SyntaxNode CloneImpl() => new UnaryMinusExpression(Operand);
    }

    public sealed class ComplementExpression : UnaryExpression
    {
        private protected override SyntaxKind Kind => SyntaxKind.BitwiseNotExpression;

        internal ComplementExpression(PrefixUnaryExpressionSyntax syntax, SyntaxNode parent)
            : base(syntax, parent) { }

        public ComplementExpression(Expression operand) : base(operand) { }

        private protected override SyntaxNode CloneImpl() => new ComplementExpression(Operand);
    }

    public sealed class NegateExpression : UnaryExpression
    {
        private protected override SyntaxKind Kind => SyntaxKind.LogicalNotExpression;

        internal NegateExpression(PrefixUnaryExpressionSyntax syntax, SyntaxNode parent)
            : base(syntax, parent) { }

        public NegateExpression(Expression operand) : base(operand) { }

        private protected override SyntaxNode CloneImpl() => new NegateExpression(Operand);
    }

    public sealed class PreIncrementExpression : UnaryExpression
    {
        internal PreIncrementExpression(PrefixUnaryExpressionSyntax syntax, SyntaxNode parent)
            : base(syntax, parent) { }

        public PreIncrementExpression(Expression operand) : base(operand) { }

        private protected override SyntaxNode CloneImpl() => new PreIncrementExpression(Operand);
    }

    public sealed class PreDecrementExpression : UnaryExpression
    {
        internal PreDecrementExpression(PrefixUnaryExpressionSyntax syntax, SyntaxNode parent)
            : base(syntax, parent) { }

        public PreDecrementExpression(Expression operand) : base(operand) { }

        private protected override SyntaxNode CloneImpl() => new PreDecrementExpression(Operand);
    }

    public sealed class AddressOfExpression : UnaryExpression
    {
        internal AddressOfExpression(PrefixUnaryExpressionSyntax syntax, SyntaxNode parent)
            : base(syntax, parent) { }

        public AddressOfExpression(Expression operand) : base(operand) { }

        private protected override SyntaxNode CloneImpl() => new AddressOfExpression(Operand);
    }

    public sealed class PointerIndirectionExpression : UnaryExpression
    {
        internal PointerIndirectionExpression(PrefixUnaryExpressionSyntax syntax, SyntaxNode parent)
            : base(syntax, parent) { }

        public PointerIndirectionExpression(Expression operand) : base(operand) { }

        private protected override SyntaxNode CloneImpl() => new PointerIndirectionExpression(Operand);
    }

    public sealed class PostIncrementExpression : UnaryExpression
    {
        private protected override bool IsPrefix => false;

        internal PostIncrementExpression(PostfixUnaryExpressionSyntax syntax, SyntaxNode parent)
            : base(syntax, parent) { }

        public PostIncrementExpression(Expression operand) : base(operand) { }

        private protected override SyntaxNode CloneImpl() => new PostIncrementExpression(Operand);
    }

    public sealed class PostDecrementExpression : UnaryExpression
    {
        private protected override bool IsPrefix => false;

        internal PostDecrementExpression(PostfixUnaryExpressionSyntax syntax, SyntaxNode parent)
            : base(syntax, parent) { }

        public PostDecrementExpression(Expression operand) : base(operand) { }

        private protected override SyntaxNode CloneImpl() => new PostDecrementExpression(Operand);
    }

    public sealed class IndexExpression : UnaryExpression
    {
        private protected override bool IsPrefix => true;

        internal IndexExpression(PrefixUnaryExpressionSyntax syntax, SyntaxNode parent)
            : base(syntax, parent) { }

        public IndexExpression(Expression operand) : base(operand) { }

        private protected override SyntaxNode CloneImpl() => new IndexExpression(Operand);
    }

    public sealed class SuppressNullableWarningExpression : UnaryExpression
    {
        private protected override bool IsPrefix => false;

        internal SuppressNullableWarningExpression(PostfixUnaryExpressionSyntax syntax, SyntaxNode parent)
            : base(syntax, parent) { }

        public SuppressNullableWarningExpression(Expression operand) : base(operand) { }

        private protected override SyntaxNode CloneImpl() => new SuppressNullableWarningExpression(Operand);
    }
}