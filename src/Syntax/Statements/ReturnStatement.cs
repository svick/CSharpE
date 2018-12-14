using System;
using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RoslynSyntaxFactory = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Roslyn = Microsoft.CodeAnalysis;

namespace CSharpE.Syntax
{
    public sealed class ReturnStatement : Statement
    {
        private ReturnStatementSyntax syntax;

        internal ReturnStatement(ReturnStatementSyntax syntax, SyntaxNode parent)
        {
            this.syntax = syntax ?? throw new ArgumentNullException(nameof(syntax));
            Parent = parent;
        }

        public ReturnStatement(Expression expression) => Expression = expression;

        public ReturnStatement() : this(null) { }

        private bool expressionSet;
        private Expression expression;
        public Expression Expression
        {
            get
            {
                if (!expressionSet)
                {
                    expression = FromRoslyn.Expression(syntax.Expression, this);
                    expressionSet = true;
                }

                return expression;
            }
            set
            {
                Set(ref expression, value);
                expressionSet = true;
            }
        }


        private protected override StatementSyntax GetWrappedStatement(ref bool? changed)
        {
            GetAndResetChanged(ref changed);

            bool? thisChanged = false;

            var newExpression = expressionSet ? expression?.GetWrapped(ref thisChanged) : syntax.Expression;

            if (syntax == null || thisChanged == true)
            {
                syntax = RoslynSyntaxFactory.ReturnStatement(newExpression);

                SetChanged(ref changed);
            }

            return syntax;
        }

        private protected override void SetSyntaxImpl(Roslyn::SyntaxNode newSyntax)
        {
            syntax = (ReturnStatementSyntax)newSyntax;
            expressionSet = false;
            Set(ref expression, null);
        }

        internal override SyntaxNode Clone() => new ReturnStatement(Expression);

        internal override SyntaxNode Parent { get; set; }
    }
}