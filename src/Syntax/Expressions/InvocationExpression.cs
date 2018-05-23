using System;
using System.Collections.Generic;
using System.Linq;
using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using CSharpSyntaxFactory = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace CSharpE.Syntax
{
    public sealed class InvocationExpression : Expression
    {
        private InvocationExpressionSyntax syntax;

        internal InvocationExpression(InvocationExpressionSyntax syntax, SyntaxNode parent)
        {
            this.syntax = syntax ?? throw new ArgumentNullException(nameof(syntax));
            Parent = parent;
        }

        public InvocationExpression(Expression expression, IEnumerable<Argument> arguments = null)
        {
            Expression = expression;
            this.arguments = new SeparatedSyntaxList<Argument, ArgumentSyntax>(arguments);
        }

        public InvocationExpression(Expression expression, IEnumerable<Expression> arguments = null)
            : this(expression, arguments?.Select(a => (Argument)a)) { }

        public InvocationExpression(Expression expression, params Argument[] arguments)
            : this(expression, (IEnumerable<Argument>)arguments) { }

        private Expression expression;
        public Expression Expression
        {
            get
            {
                if (expression == null)
                    expression = FromRoslyn.Expression(syntax.Expression);
                return expression;
            }
            set => expression = value ?? throw new ArgumentNullException(nameof(value));
        }

        private SeparatedSyntaxList<Argument, ArgumentSyntax> arguments;
        public IList<Argument> Arguments
        {
            get
            {
                if (arguments == null)
                    arguments = new SeparatedSyntaxList<Argument, ArgumentSyntax>(syntax.ArgumentList.Arguments);

                return arguments;
            }
            set => arguments = new SeparatedSyntaxList<Argument, ArgumentSyntax>(value);
        }

        internal override ExpressionSyntax GetWrapped()
        {
            var newExpression = expression?.GetWrapped() ?? syntax.Expression;
            var newArguments = arguments?.GetWrapped() ?? syntax.ArgumentList.Arguments;

            if (syntax == null || newExpression != syntax.Expression || newArguments != syntax.ArgumentList.Arguments)
            {
                syntax = CSharpSyntaxFactory.InvocationExpression(
                    newExpression, CSharpSyntaxFactory.ArgumentList(newArguments));
            }

            return syntax;
        }

        internal override SyntaxNode Parent { get; set; }
    }
}