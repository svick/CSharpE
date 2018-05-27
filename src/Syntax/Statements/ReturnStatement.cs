﻿using System;
using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using CSharpSyntaxFactory = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
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


        internal new ReturnStatementSyntax GetWrapped()
        {
            var newExpression = expressionSet ? expression?.GetWrapped() : syntax.Expression;

            if (syntax == null || newExpression != syntax.Expression)
            {
                syntax = CSharpSyntaxFactory.ReturnStatement(newExpression);
            }

            return syntax;
        }

        protected override StatementSyntax GetWrappedImpl() => GetWrapped();

        protected override void SetSyntaxImpl(Roslyn::SyntaxNode newSyntax)
        {
            syntax = (ReturnStatementSyntax)newSyntax;
            expressionSet = false;
            Set(ref expression, null);
        }

        internal override SyntaxNode Clone() => new ReturnStatement(Expression);

        internal override SyntaxNode Parent { get; set; }
    }
}