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
    public abstract class VariableInitializer : SyntaxNode, ISyntaxWrapper<ExpressionSyntax>
    {
        private protected VariableInitializer() { }
        private protected VariableInitializer(ExpressionSyntax syntax) : base(syntax) { }

        ExpressionSyntax ISyntaxWrapper<ExpressionSyntax>.GetWrapped(ref bool? changed) => GetWrappedExpression(ref changed);

        internal abstract ExpressionSyntax GetWrappedExpression(ref bool? changed);

        public abstract void ReplaceExpressions<T>(Func<T, bool> filter, Func<T, Expression> projection)
            where T : Expression;
    }

    public sealed class ArrayInitializer : VariableInitializer
    {
        private InitializerExpressionSyntax syntax;

        internal ArrayInitializer(InitializerExpressionSyntax syntax, SyntaxNode parent)
            : base(syntax)
        {
            Debug.Assert(syntax.Kind() == SyntaxKind.ArrayInitializerExpression);

            this.syntax = syntax;
            Parent = parent;
        }

        public ArrayInitializer(params VariableInitializer[] variableInitializers)
            : this(variableInitializers.AsEnumerable()) { }

        public ArrayInitializer(IEnumerable<VariableInitializer> variableInitializers) =>
            this.variableInitializers = new VariableInitializerList(variableInitializers, this);

        private VariableInitializerList variableInitializers;
        public IList<VariableInitializer> VariableInitializers
        {
            get
            {
                if (variableInitializers == null)
                    variableInitializers = new VariableInitializerList(syntax.Expressions, this);

                return variableInitializers;
            }
            set => SetList(ref variableInitializers, new VariableInitializerList(value, this));
        }

        internal override ExpressionSyntax GetWrappedExpression(ref bool? changed) => GetWrapped(ref changed);

        internal InitializerExpressionSyntax GetWrapped(ref bool? changed)
        {
            GetAndResetChanged(ref changed, out var thisChanged);

            var newElementInitializers = variableInitializers?.GetWrapped(ref thisChanged) ?? syntax.Expressions;

            if (syntax == null || thisChanged == true)
            {
                syntax = RoslynSyntaxFactory.InitializerExpression(
                    SyntaxKind.ArrayInitializerExpression, newElementInitializers);

                SetChanged(ref changed);
            }

            return syntax;
        }

        private protected override void SetSyntaxImpl(Roslyn::SyntaxNode newSyntax)
        {
            SetList(ref variableInitializers, null);
            syntax = (InitializerExpressionSyntax)newSyntax;
        }

        private protected override SyntaxNode CloneImpl() => new ArrayInitializer(VariableInitializers);

        public override IEnumerable<SyntaxNode> GetChildren() => VariableInitializers;

        public override void ReplaceExpressions<T>(Func<T, bool> filter, Func<T, Expression> projection)
        {
            foreach (var initializer in VariableInitializers)
            {
                initializer.ReplaceExpressions(filter, projection);
            }
        }
    }

    public sealed class ExpressionVariableInitializer : VariableInitializer
    {
        private ExpressionSyntax syntax;

        internal ExpressionVariableInitializer(ExpressionSyntax syntax, SyntaxNode parent)
            : base(syntax)
        {
            this.syntax = syntax;
            Parent = parent;
        }

        public ExpressionVariableInitializer(Expression expression) => Expression = expression;

        private Expression expression;
        public Expression Expression
        {
            get
            {
                if (expression == null)
                    expression = FromRoslyn.Expression(syntax, this);

                return expression;
            }
            set => SetNotNull(ref expression, value);
        }

        internal override ExpressionSyntax GetWrappedExpression(ref bool? changed)
        {
            GetAndResetChanged(ref changed, out var thisChanged);

            var newExpression = expression?.GetWrapped(ref thisChanged) ?? syntax;

            if (syntax == null || thisChanged == true)
            {
                syntax = newExpression;

                SetChanged(ref changed);
            }

            return syntax;
        }

        private protected override void SetSyntaxImpl(Roslyn::SyntaxNode newSyntax)
        {
            Set(ref expression, null);
            syntax = (ExpressionSyntax)newSyntax;
        }

        private protected override SyntaxNode CloneImpl() => new ExpressionVariableInitializer(Expression);

        public override void ReplaceExpressions<T>(Func<T, bool> filter, Func<T, Expression> projection) =>
            Expression = Expression.ReplaceExpressions(Expression, filter, projection);
    }
}
