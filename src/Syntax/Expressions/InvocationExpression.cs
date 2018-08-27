using System;
using System.Collections.Generic;
using System.Linq;
using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using CSharpSyntaxFactory = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Roslyn = Microsoft.CodeAnalysis;

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
            this.arguments = new SeparatedSyntaxList<Argument, ArgumentSyntax>(arguments, this);
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
                    expression = FromRoslyn.Expression(syntax.Expression, this);

                return expression;
            }
            set => SetNotNull(ref expression, value);
        }

        private SeparatedSyntaxList<Argument, ArgumentSyntax> arguments;
        public IList<Argument> Arguments
        {
            get
            {
                if (arguments == null)
                    arguments = new SeparatedSyntaxList<Argument, ArgumentSyntax>(syntax.ArgumentList.Arguments, this);

                return arguments;
            }
            set => SetList(ref arguments, new SeparatedSyntaxList<Argument, ArgumentSyntax>(value, this));
        }

        internal override ExpressionSyntax GetWrapped(ref bool? changed)
        {
            GetAndResetChanged(ref changed);

            bool? thisChanged = false;

            var newExpression = expression?.GetWrapped(ref thisChanged) ?? syntax.Expression;
            var newArguments = arguments?.GetWrapped(ref thisChanged) ?? syntax.ArgumentList.Arguments;

            if (syntax == null || thisChanged == true)
            {
                syntax = CSharpSyntaxFactory.InvocationExpression(
                    newExpression, CSharpSyntaxFactory.ArgumentList(newArguments));

                SetChanged(ref changed);
            }

            return syntax;
        }

        private protected override void SetSyntaxImpl(Roslyn::SyntaxNode newSyntax)
        {
            syntax = (InvocationExpressionSyntax)newSyntax;

            Set(ref expression, null);
            arguments = null;
        }

        internal override SyntaxNode Clone() => new InvocationExpression(Expression, Arguments);

        internal override SyntaxNode Parent { get; set; }
    }
}