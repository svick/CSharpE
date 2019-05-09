using System;
using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RoslynSyntaxFactory = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Roslyn = Microsoft.CodeAnalysis;

namespace CSharpE.Syntax
{
    public abstract class UnaryExpression : Expression
    {
        private ExpressionSyntax syntax;

        internal UnaryExpression(ExpressionSyntax syntax, SyntaxNode parent)
            : base(syntax)
        {
            this.syntax = syntax;
            Parent = parent;
        }

        internal UnaryExpression(Expression operand) => Operand = operand;

        ExpressionSyntax GetOperand(ExpressionSyntax expression)
        {
            switch (expression)
            {
                case PrefixUnaryExpressionSyntax prefix: return prefix.Operand;
                case PostfixUnaryExpressionSyntax postfix: return postfix.Operand;
                default: throw new InvalidOperationException();
            }
        }

        private Expression operand;
        public Expression Operand
        {
            get
            {
                if (operand == null)
                    operand = FromRoslyn.Expression(GetOperand(syntax), this);

                return operand;
            }
            set => SetNotNull(ref operand, value);
        }

        // PERF: consider caching the parsed value?
        private protected virtual SyntaxKind Kind =>
            Enum.TryParse(GetType().Name, out SyntaxKind kind) ? kind : throw new InvalidOperationException();

        private protected virtual bool IsPrefix => true;

        private protected sealed override ExpressionSyntax GetWrappedExpression(ref bool? changed)
        {
            GetAndResetChanged(ref changed);

            bool? thisChanged = false;

            var newOperand = operand?.GetWrapped(ref thisChanged) ?? GetOperand(syntax);

            if (syntax == null || thisChanged == true || ShouldAnnotate(syntax, changed))
            {
                syntax = IsPrefix 
                    ? (ExpressionSyntax)RoslynSyntaxFactory.PrefixUnaryExpression(Kind, newOperand)
                    : RoslynSyntaxFactory.PostfixUnaryExpression(Kind, newOperand);

                syntax = Annotate(syntax);

                SetChanged(ref changed);
            }

            return syntax;
        }

        private protected override void SetSyntaxImpl(Roslyn::SyntaxNode newSyntax)
        {
            Set(ref operand, null);
            syntax = (ExpressionSyntax)newSyntax;
        }
    }
}