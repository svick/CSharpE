using System;
using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RoslynSyntaxFactory = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Roslyn = Microsoft.CodeAnalysis;

namespace CSharpE.Syntax
{
    public sealed class SwitchArm : SyntaxNode, ISyntaxWrapper<SwitchExpressionArmSyntax>
    {
        private SwitchExpressionArmSyntax syntax;

        internal SwitchArm(SwitchExpressionArmSyntax syntax, SyntaxNode parent) : base(syntax)
        {
            this.syntax = syntax;
            Parent = parent;
        }

        public SwitchArm(Pattern pattern, Expression expression)
            : this(pattern, null, expression) { }

        public SwitchArm(Pattern pattern, Expression whenCondition, Expression expression)
        {
            Pattern = pattern;
            WhenCondition = whenCondition;
            Expression = expression;
        }

        private Pattern pattern;
        public Pattern Pattern
        {
            get
            {
                if (pattern == null)
                    pattern = FromRoslyn.Pattern(syntax.Pattern, this);

                return pattern;
            }
            set => SetNotNull(ref pattern, value);
        }

        private bool whenConditionSet;
        private Expression whenCondition;
        public Expression WhenCondition
        {
            get
            {
                if (!whenConditionSet)
                {
                    whenCondition = FromRoslyn.Expression(syntax.WhenClause?.Condition, this);
                    whenConditionSet = true;
                }

                return whenCondition;
            }
            set
            {
                Set(ref whenCondition, value);
                whenConditionSet = true;
            }
        }

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

        SwitchExpressionArmSyntax ISyntaxWrapper<SwitchExpressionArmSyntax>.GetWrapped(ref bool? changed)
        {
            GetAndResetChanged(ref changed, out var thisChanged);

            var newPattern = pattern?.GetWrapped(ref thisChanged) ?? syntax.Pattern;
            var newWhenCondition = whenConditionSet ? whenCondition?.GetWrapped(ref thisChanged) : syntax.WhenClause?.Condition;
            var newExpression = expression?.GetWrapped(ref thisChanged) ?? syntax.Expression;

            if (syntax == null || thisChanged == true || ShouldAnnotate(syntax, changed))
            {
                var whenClause = newWhenCondition == null ? null : RoslynSyntaxFactory.WhenClause(newWhenCondition);

                syntax = RoslynSyntaxFactory.SwitchExpressionArm(newPattern, whenClause, newExpression);

                syntax = Annotate(syntax);

                SetChanged(ref changed);
            }

            return syntax;
        }

        private protected override void SetSyntaxImpl(Roslyn::SyntaxNode newSyntax)
        {
            syntax = (SwitchExpressionArmSyntax)newSyntax;

            Set(ref pattern, null);
            Set(ref whenCondition, null);
            whenConditionSet = false;
            Set(ref expression, null);
        }

        private protected override SyntaxNode CloneImpl() => new SwitchArm(Pattern, WhenCondition, Expression);

        public void ReplaceExpressions<T>(Func<T, bool> filter, Func<T, Expression> projection) where T : Expression
        {
            Pattern.ReplaceExpressions(filter, projection);
            Expression = Expression.ReplaceExpressions(Expression, filter, projection);
        }
    }
}