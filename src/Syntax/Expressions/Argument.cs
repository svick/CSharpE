using System;
using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using CSharpSyntaxFactory = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace CSharpE.Syntax
{
    // TODO: named arguments; ref and out
    public class Argument : ISyntaxWrapper<ArgumentSyntax>
    {
        private ArgumentSyntax syntax;

        internal Argument(ArgumentSyntax syntax) =>
            this.syntax = syntax ?? throw new ArgumentNullException(nameof(syntax));

        public Argument(Expression expression) =>
            this.expression = expression ?? throw new ArgumentNullException(nameof(expression));

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

        public ArgumentSyntax GetWrapped(WrapperContext context)
        {
            var newExpression = expression?.GetWrapped(context) ?? syntax.Expression;

            if (syntax == null || newExpression != syntax.Expression)
            {
                syntax = CSharpSyntaxFactory.Argument(newExpression);
            }

            return syntax;
        }

        public static implicit operator Argument(Expression expression) => new Argument(expression);
    }
}