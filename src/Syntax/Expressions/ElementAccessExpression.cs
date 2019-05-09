using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RoslynSyntaxFactory = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Roslyn = Microsoft.CodeAnalysis;

namespace CSharpE.Syntax
{
    public abstract class BaseElementAccessExpression : Expression
    {
        private ExpressionSyntax syntax;

        internal BaseElementAccessExpression(ExpressionSyntax syntax, SyntaxNode parent)
        {
            Debug.Assert(
                syntax is ElementAccessExpressionSyntax ||
                (syntax is ConditionalAccessExpressionSyntax conditionalAccess &&
                 conditionalAccess.WhenNotNull is ElementBindingExpressionSyntax));

            this.syntax = syntax;
            Parent = parent;
        }

        internal BaseElementAccessExpression(Expression expression, IEnumerable<Argument> arguments)
        {
            Expression = expression;
            this.arguments = new SeparatedSyntaxList<Argument, ArgumentSyntax>(arguments, this);
        }

        private ExpressionSyntax GetExpressionSyntax()
        {
            switch (syntax)
            {
                case ElementAccessExpressionSyntax memberAccess:
                    return memberAccess.Expression;
                case ConditionalAccessExpressionSyntax conditionalAccess:
                    return conditionalAccess.Expression;
                default:
                    throw new InvalidOperationException();
            }
        }

        private Expression expression;
        public Expression Expression
        {
            get
            {
                if (expression == null)
                    expression = FromRoslyn.Expression(GetExpressionSyntax(), this);

                return expression;
            }
            set => SetNotNull(ref expression, value);
        }

        private Roslyn::SeparatedSyntaxList<ArgumentSyntax> GetArgumentsSyntax()
        {
            switch (syntax)
            {
                case ElementAccessExpressionSyntax memberAccess:
                    return memberAccess.ArgumentList.Arguments;
                case ConditionalAccessExpressionSyntax conditionalAccess:
                    var elementBinding = (ElementBindingExpressionSyntax)conditionalAccess.WhenNotNull;
                    return elementBinding.ArgumentList.Arguments;
                default:
                    throw new InvalidOperationException();
            }
        }

        private SeparatedSyntaxList<Argument, ArgumentSyntax> arguments;
        public IList<Argument> Arguments
        {
            get
            {
                if (arguments == null)
                    arguments = new SeparatedSyntaxList<Argument, ArgumentSyntax>(GetArgumentsSyntax(), this);

                return arguments;
            }
            set => SetList(ref arguments, new SeparatedSyntaxList<Argument, ArgumentSyntax>(value, this));
        }

        private protected override ExpressionSyntax GetWrappedExpression(ref bool? changed)
        {
            GetAndResetChanged(ref changed);

            bool? thisChanged = false;

            var newExpression = expression?.GetWrapped(ref thisChanged) ?? GetExpressionSyntax();
            var newArguments = arguments?.GetWrapped(ref thisChanged) ?? GetArgumentsSyntax();

            if (syntax == null || thisChanged == true || ShouldAnnotate(syntax, changed))
            {
                var argumentsSyntax = RoslynSyntaxFactory.BracketedArgumentList(newArguments);

                if (this is ConditionalElementAccessExpression)
                {
                    syntax = RoslynSyntaxFactory.ConditionalAccessExpression(
                        newExpression, RoslynSyntaxFactory.ElementBindingExpression(argumentsSyntax));
                }
                else
                {
                    syntax = RoslynSyntaxFactory.ElementAccessExpression(newExpression, argumentsSyntax);
                }

                syntax = Annotate(syntax);

                SetChanged(ref changed);
            }

            return syntax;
        }

        private protected override void SetSyntaxImpl(Roslyn::SyntaxNode newSyntax)
        {
            syntax = (ExpressionSyntax)newSyntax;

            Set(ref expression, null);
        }

        public override IEnumerable<SyntaxNode> GetChildren() => new SyntaxNode[] { Expression }.Concat(Arguments);

        public override void ReplaceExpressions<T>(Func<T, bool> filter, Func<T, Expression> projection)
        {
            base.ReplaceExpressions(filter, projection);

            foreach (var argument in Arguments)
            {
                argument.ReplaceExpressions(filter, projection);
            }
        }
    }

    public class ElementAccessExpression : BaseElementAccessExpression
    {
        internal ElementAccessExpression(ElementAccessExpressionSyntax syntax, SyntaxNode parent)
            : base(syntax, parent) { }

        public ElementAccessExpression(Expression expression, params Argument[] arguments)
            : base(expression, arguments) { }

        public ElementAccessExpression(Expression expression, IEnumerable<Argument> arguments)
            : base(expression, arguments) { }

        private protected override SyntaxNode CloneImpl() => new ElementAccessExpression(Expression, Arguments);
    }

    public class ConditionalElementAccessExpression : BaseElementAccessExpression
    {
        internal ConditionalElementAccessExpression(ConditionalAccessExpressionSyntax syntax, SyntaxNode parent)
            : base(syntax, parent)
            => Debug.Assert(syntax.WhenNotNull is ElementBindingExpressionSyntax);

        public ConditionalElementAccessExpression(Expression expression, params Argument[] arguments)
            : base(expression, arguments) { }

        public ConditionalElementAccessExpression(Expression expression, IEnumerable<Argument> arguments)
            : base(expression, arguments) { }

        private protected override SyntaxNode CloneImpl() => new ConditionalElementAccessExpression(Expression, Arguments);
    }
}