using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSharpE.Syntax
{
    public sealed class UnaryPlusExpression : UnaryExpression
    {
        private protected override SyntaxKind Kind => SyntaxKind.UnaryPlusExpression;

        internal UnaryPlusExpression(PrefixUnaryExpressionSyntax syntax, SyntaxNode parent)
            : base(syntax, parent) { }

        public UnaryPlusExpression(Expression operand) : base(operand) { }

        internal override SyntaxNode Clone() => new UnaryPlusExpression(Operand);
    }

    public sealed class UnaryMinusExpression : UnaryExpression
    {
        private protected override SyntaxKind Kind => SyntaxKind.UnaryMinusExpression;

        internal UnaryMinusExpression(PrefixUnaryExpressionSyntax syntax, SyntaxNode parent)
            : base(syntax, parent) { }

        public UnaryMinusExpression(Expression operand) : base(operand) { }

        internal override SyntaxNode Clone() => new UnaryMinusExpression(Operand);
    }

    public sealed class ComplementExpression : UnaryExpression
    {
        private protected override SyntaxKind Kind => SyntaxKind.BitwiseNotExpression;

        internal ComplementExpression(PrefixUnaryExpressionSyntax syntax, SyntaxNode parent)
            : base(syntax, parent) { }

        public ComplementExpression(Expression operand) : base(operand) { }

        internal override SyntaxNode Clone() => new ComplementExpression(Operand);
    }

    public sealed class NegateExpression : UnaryExpression
    {
        private protected override SyntaxKind Kind => SyntaxKind.LogicalNotExpression;

        internal NegateExpression(PrefixUnaryExpressionSyntax syntax, SyntaxNode parent)
            : base(syntax, parent) { }

        public NegateExpression(Expression operand) : base(operand) { }

        internal override SyntaxNode Clone() => new NegateExpression(Operand);
    }

    public sealed class PreIncrementExpression : UnaryExpression
    {
        private protected override SyntaxKind Kind => SyntaxKind.PreIncrementExpression;

        internal PreIncrementExpression(PrefixUnaryExpressionSyntax syntax, SyntaxNode parent)
            : base(syntax, parent) { }

        public PreIncrementExpression(Expression operand) : base(operand) { }

        internal override SyntaxNode Clone() => new PreIncrementExpression(Operand);
    }

    public sealed class PreDecrementExpression : UnaryExpression
    {
        private protected override SyntaxKind Kind => SyntaxKind.PreDecrementExpression;

        internal PreDecrementExpression(PrefixUnaryExpressionSyntax syntax, SyntaxNode parent)
            : base(syntax, parent) { }

        public PreDecrementExpression(Expression operand) : base(operand) { }

        internal override SyntaxNode Clone() => new PreDecrementExpression(Operand);
    }

    public sealed class AddressOfExpression : UnaryExpression
    {
        private protected override SyntaxKind Kind => SyntaxKind.AddressOfExpression;

        internal AddressOfExpression(PrefixUnaryExpressionSyntax syntax, SyntaxNode parent)
            : base(syntax, parent) { }

        public AddressOfExpression(Expression operand) : base(operand) { }

        internal override SyntaxNode Clone() => new AddressOfExpression(Operand);
    }

    public sealed class PointerIndirectionExpression : UnaryExpression
    {
        private protected override SyntaxKind Kind => SyntaxKind.PointerIndirectionExpression;

        internal PointerIndirectionExpression(PrefixUnaryExpressionSyntax syntax, SyntaxNode parent)
            : base(syntax, parent) { }

        public PointerIndirectionExpression(Expression operand) : base(operand) { }

        internal override SyntaxNode Clone() => new PointerIndirectionExpression(Operand);
    }

    public sealed class PostIncrementExpression : UnaryExpression
    {
        private protected override SyntaxKind Kind => SyntaxKind.PostIncrementExpression;

        private protected override bool IsPrefix => false;

        internal PostIncrementExpression(PostfixUnaryExpressionSyntax syntax, SyntaxNode parent)
            : base(syntax, parent) { }

        public PostIncrementExpression(Expression operand) : base(operand) { }

        internal override SyntaxNode Clone() => new PostIncrementExpression(Operand);
    }

    public sealed class PostDecrementExpression : UnaryExpression
    {
        private protected override SyntaxKind Kind => SyntaxKind.PostDecrementExpression;

        private protected override bool IsPrefix => false;

        internal PostDecrementExpression(PostfixUnaryExpressionSyntax syntax, SyntaxNode parent)
            : base(syntax, parent) { }

        public PostDecrementExpression(Expression operand) : base(operand) { }

        internal override SyntaxNode Clone() => new PostDecrementExpression(Operand);
    }
}