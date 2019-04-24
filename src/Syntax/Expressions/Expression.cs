using System;
using System.Collections.Generic;
using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSharpE.Syntax
{
    public abstract class Expression : SyntaxNode, ISyntaxWrapper<ExpressionSyntax>
    {
        private protected Expression() { }

        private protected Expression(ExpressionSyntax syntax) : base(syntax) { }

        public static implicit operator ExpressionStatement(Expression expression) =>
            new ExpressionStatement(expression);

        private protected abstract ExpressionSyntax GetWrappedExpression(ref bool? changed);

        ExpressionSyntax ISyntaxWrapper<ExpressionSyntax>.GetWrapped(ref bool? changed) =>
            GetWrappedExpression(ref changed);

        public TypeReference GetExpressionType() =>
            FromRoslyn.TypeReference(SourceFile?.SemanticModel.GetTypeInfo((ExpressionSyntax)GetSourceFileNode()).Type);

        internal static Expression ReplaceExpressions<T>(
            Expression expression, Func<T, bool> filter, Func<T, Expression> projection) where T : Expression
        {
            if (expression == null)
                return null;

            if (expression is T matchedExpression && filter(matchedExpression))
            {
                // this has to be called after filter but before projection, to make it work correctly,
                // even though doing it this way is likely going to be confusing
                expression.ReplaceExpressions(filter, projection);

                return projection(matchedExpression);
            }

            expression.ReplaceExpressions(filter, projection);

            return expression;
        }

        public virtual void ReplaceExpressions<T>(Func<T, bool> filter, Func<T, Expression> projection) where T : Expression
        {
            // PERF
            foreach (var property in this.GetType().GetProperties())
            {
                var propertyType = property.PropertyType;

                if (typeof(Expression).IsAssignableFrom(propertyType))
                {
                    var oldValue = (Expression)property.GetValue(this);

                    var newValue = ReplaceExpressions(oldValue, filter, projection);

                    property.SetValue(this, newValue);
                }
                else if (typeof(IList<Expression>).IsAssignableFrom(propertyType))
                {
                    var list = (IList<Expression>)property.GetValue(this);

                    for (int i = 0; i < list.Count; i++)
                    {
                        list[i] = ReplaceExpressions(list[i], filter, projection);
                    }
                }
            }
        }
    }
}