using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RoslynSyntaxFactory = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Roslyn = Microsoft.CodeAnalysis;

namespace CSharpE.Syntax
{
    public sealed class ElementInitializer : SyntaxNode, ISyntaxWrapper<ExpressionSyntax>
    {
        private ExpressionSyntax syntax;

        internal ElementInitializer(ExpressionSyntax syntax, CollectionInitializer parent)
            : base(syntax)
        {
            this.syntax = syntax;
            Parent = parent;
        }

        public ElementInitializer(params Expression[] expressions) : this(expressions.AsEnumerable()) { }

        public ElementInitializer(IEnumerable<Expression> expressions) =>
            this.expressions = new ExpressionList(expressions, this);

        private Roslyn::SeparatedSyntaxList<ExpressionSyntax> GetSyntaxExpressions()
        {
            if (syntax is InitializerExpressionSyntax initializerExpression)
            {
                Debug.Assert(initializerExpression.Kind() == SyntaxKind.ComplexElementInitializerExpression);
                return initializerExpression.Expressions;
            }

            return RoslynSyntaxFactory.SingletonSeparatedList(syntax);
        }

        private ExpressionList expressions;
        public IList<Expression> Expressions
        {
            get
            {
                if (expressions == null)
                    expressions = new ExpressionList(GetSyntaxExpressions(), this);

                return expressions;
            }
            set => SetList(
                ref expressions,
                new ExpressionList(value, this));
        }

        ExpressionSyntax ISyntaxWrapper<ExpressionSyntax>.GetWrapped(ref bool? changed)
        {
            GetAndResetChanged(ref changed, out var thisChanged);

            var newExpressions = expressions?.GetWrapped(ref thisChanged) ?? GetSyntaxExpressions();

            if (syntax == null || thisChanged == true)
            {
                var firstExpression = newExpressions.FirstOrDefault();

                if (newExpressions.Count == 1 && !SyntaxFacts.IsAssignmentExpression(firstExpression.Kind()))
                {
                    syntax = firstExpression;
                }
                else
                {
                    syntax = RoslynSyntaxFactory.InitializerExpression(
                        SyntaxKind.ComplexElementInitializerExpression, newExpressions);
                }

                SetChanged(ref changed);
            }

            return syntax;
        }

        private protected override void SetSyntaxImpl(Roslyn::SyntaxNode newSyntax)
        {
            SetList(ref expressions, null);
            syntax = (AssignmentExpressionSyntax)newSyntax;
        }

        private protected override SyntaxNode CloneImpl() => new ElementInitializer(Expressions);

        public override IEnumerable<SyntaxNode> GetChildren() => Expressions;

        public void ReplaceExpressions<T>(Func<T, bool> filter, Func<T, Expression> projection) where T : Expression
        {
            for (int i = 0; i < Expressions.Count; i++)
            {
                Expressions[i] = Expression.ReplaceExpressions(Expressions[i], filter, projection);
            }
        }
    }
}
