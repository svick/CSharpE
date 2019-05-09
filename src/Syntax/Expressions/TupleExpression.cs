using System;
using System.Collections.Generic;
using System.Linq;
using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RoslynSyntaxFactory = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Roslyn = Microsoft.CodeAnalysis;

namespace CSharpE.Syntax
{
    public sealed class TupleExpression : Expression, ISyntaxWrapper<TupleExpressionSyntax>
    {
        private TupleExpressionSyntax syntax;

        internal TupleExpression(TupleExpressionSyntax syntax, SyntaxNode parent)
            : base(syntax)
        {
            this.syntax = syntax;
            Parent = parent;
        }

        public TupleExpression(params Expression[] expressions) : this(expressions.AsEnumerable()) { }

        public TupleExpression(IEnumerable<Expression> expressions)
            : this(expressions?.Select(e => (Argument)e)) { }

        public TupleExpression(IEnumerable<Argument> arguments) => Arguments = arguments?.ToList();

        private SeparatedSyntaxList<Argument, ArgumentSyntax> arguments;
        public IList<Argument> Arguments
        {
            get
            {
                if (arguments == null)
                    arguments = new SeparatedSyntaxList<Argument, ArgumentSyntax>(syntax.Arguments, this);

                return arguments;
            }
            set => SetList(ref arguments, new SeparatedSyntaxList<Argument, ArgumentSyntax>(value, this));
        }

        TupleExpressionSyntax ISyntaxWrapper<TupleExpressionSyntax>.GetWrapped(ref bool? changed)
        {
            GetAndResetChanged(ref changed);

            bool? thisChanged = false;

            var newArguments = arguments?.GetWrapped(ref thisChanged) ?? syntax.Arguments;

            if (syntax == null || thisChanged == true || ShouldAnnotate(syntax, changed))
            {
                syntax = RoslynSyntaxFactory.TupleExpression(newArguments);

                syntax = Annotate(syntax);

                SetChanged(ref changed);
            }

            return syntax;
        }

        private protected override ExpressionSyntax GetWrappedExpression(ref bool? changed) =>
            this.GetWrapped<TupleExpressionSyntax>(ref changed);

        private protected override void SetSyntaxImpl(Roslyn::SyntaxNode newSyntax) => syntax = (TupleExpressionSyntax)newSyntax;

        private protected override SyntaxNode CloneImpl() => new TupleExpression(Arguments);

        public override void ReplaceExpressions<T>(Func<T, bool> filter, Func<T, Expression> projection)
        {
            foreach (var argument in Arguments)
            {
                argument.ReplaceExpressions(filter, projection);
            }
        }
    }
}