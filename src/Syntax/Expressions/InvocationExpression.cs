using System;
using System.Collections.Generic;
using System.Linq;
using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RoslynSyntaxFactory = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Roslyn = Microsoft.CodeAnalysis;

namespace CSharpE.Syntax
{
    public sealed class InvocationExpression : Expression, ISyntaxWrapper<InvocationExpressionSyntax>
    {
        private InvocationExpressionSyntax syntax;

        internal InvocationExpression(InvocationExpressionSyntax syntax, SyntaxNode parent)
            : base(syntax)
        {
            this.syntax = syntax;
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

        InvocationExpressionSyntax ISyntaxWrapper<InvocationExpressionSyntax>.GetWrapped(ref bool? changed)
        {
            GetAndResetChanged(ref changed);

            bool? thisChanged = false;

            var newExpression = expression?.GetWrapped(ref thisChanged) ?? syntax.Expression;
            var newArguments = arguments?.GetWrapped(ref thisChanged) ?? syntax.ArgumentList.Arguments;

            if (syntax == null || thisChanged == true || ShouldAnnotate(syntax, changed))
            {
                syntax = RoslynSyntaxFactory.InvocationExpression(
                    newExpression, RoslynSyntaxFactory.ArgumentList(newArguments));

                syntax = Annotate(syntax);

                SetChanged(ref changed);
            }

            return syntax;
        }

        private protected override ExpressionSyntax GetWrappedExpression(ref bool? changed) =>
            this.GetWrapped<InvocationExpressionSyntax>(ref changed);

        private protected override void SetSyntaxImpl(Roslyn::SyntaxNode newSyntax)
        {
            syntax = (InvocationExpressionSyntax)newSyntax;

            Set(ref expression, null);
            arguments = null;
        }

        private protected override SyntaxNode CloneImpl() => new InvocationExpression(Expression, Arguments);

        public override IEnumerable<SyntaxNode> GetChildren() => new SyntaxNode[] { Expression }.Concat(Arguments);

        public override void ReplaceExpressions<T>(Func<T, bool> filter, Func<T, Expression> projection)
        {
            Expression = ReplaceExpressions(Expression, filter, projection);

            foreach (var argument in Arguments)
            {
                argument.ReplaceExpressions(filter, projection);
            }
        }
    }
}