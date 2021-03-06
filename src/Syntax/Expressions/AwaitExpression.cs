using System;
using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RoslynSyntaxFactory = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Roslyn = Microsoft.CodeAnalysis;

namespace CSharpE.Syntax
{
    public sealed class AwaitExpression : Expression
    {
        private AwaitExpressionSyntax syntax;

        internal AwaitExpression(AwaitExpressionSyntax syntax, SyntaxNode parent)
            : base(syntax)
        {
            this.syntax = syntax;
            Parent = parent;
        }

        public AwaitExpression(Expression operand) =>
            Operand = operand ?? throw new ArgumentNullException(nameof(operand));

        private Expression operand;
        public Expression Operand
        {
            get
            {
                if (operand == null)
                    operand = FromRoslyn.Expression(syntax.Expression, this);

                return operand;
            }
            set => SetNotNull(ref operand, value);
        }

        private protected override ExpressionSyntax GetWrappedExpression(ref bool? changed)
        {
            GetAndResetChanged(ref changed, out var thisChanged);

            var newOperand = operand?.GetWrapped(ref thisChanged) ?? syntax.Expression;

            if (syntax == null || thisChanged == true || ShouldAnnotate(syntax, changed))
            {
                syntax = RoslynSyntaxFactory.AwaitExpression(newOperand);

                syntax = Annotate(syntax);

                SetChanged(ref changed);
            }

            return syntax;
        }

        private protected override void SetSyntaxImpl(Roslyn::SyntaxNode newSyntax)
        {
            syntax = (AwaitExpressionSyntax)newSyntax;

            Set(ref operand, null);
        }

        private protected override SyntaxNode CloneImpl() => new AwaitExpression(Operand);
    }
}