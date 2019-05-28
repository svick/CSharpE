using System;
using System.Diagnostics;
using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RoslynSyntaxFactory = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Roslyn = Microsoft.CodeAnalysis;

namespace CSharpE.Syntax
{
    public abstract class SwitchLabel : SyntaxNode, ISyntaxWrapper<SwitchLabelSyntax>
    {
        private protected SwitchLabel() { }
        private protected SwitchLabel(SwitchLabelSyntax syntax) : base(syntax) { }

        internal abstract SwitchLabelSyntax GetWrapped(ref bool? changed);

        SwitchLabelSyntax ISyntaxWrapper<SwitchLabelSyntax>.GetWrapped(ref bool? changed)
            => GetWrapped(ref changed);

        public abstract void ReplaceExpressions<T>(Func<T, bool> filter, Func<T, Expression> projection)
            where T : Expression;
    }

    public sealed class SwitchCase : SwitchLabel
    {
        private SwitchLabelSyntax syntax;

        internal SwitchCase(SwitchLabelSyntax syntax, SwitchSection parent)
            : base(syntax)
        {
            Debug.Assert(syntax is CaseSwitchLabelSyntax || syntax is CasePatternSwitchLabelSyntax);

            this.syntax = syntax;
            Parent = parent;
        }

        public SwitchCase(Expression constantPatternValue)
            : this(new ConstantPattern(constantPatternValue)) { }

        public SwitchCase(Pattern pattern, Expression whenCondition = null)
        {
            Pattern = pattern;
            WhenCondition = whenCondition;
        }

        private PatternSyntax GetSyntaxPattern()
        {
            switch (syntax)
            {
                case CaseSwitchLabelSyntax @case:
                    return RoslynSyntaxFactory.ConstantPattern(@case.Value);
                case CasePatternSwitchLabelSyntax casePattern:
                    return casePattern.Pattern;
            }

            throw new InvalidOperationException();
        }

        private Pattern pattern;
        public Pattern Pattern
        {
            get
            {
                if (pattern == null)
                    pattern = FromRoslyn.Pattern(GetSyntaxPattern(), this);

                return pattern;
            }
            set => SetNotNull(ref pattern, value);
        }

        private ExpressionSyntax GetSyntaxWhenCondition()
        {
            switch (syntax)
            {
                case CaseSwitchLabelSyntax _:
                    return null;
                case CasePatternSwitchLabelSyntax casePattern:
                    return casePattern.WhenClause?.Condition;
            }

            throw new InvalidOperationException();
        }

        private bool whenConditionSet;
        private Expression whenCondition;
        public Expression WhenCondition
        {
            get
            {
                if (!whenConditionSet)
                {
                    whenCondition = FromRoslyn.Expression(GetSyntaxWhenCondition(), this);
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

        internal override SwitchLabelSyntax GetWrapped(ref bool? changed)
        {
            GetAndResetChanged(ref changed, out var thisChanged);

            var newPattern = pattern?.GetWrapped(ref thisChanged) ?? GetSyntaxPattern();
            var newWhenCondition =
                whenConditionSet ? whenCondition?.GetWrapped(ref thisChanged) : GetSyntaxWhenCondition();

            if (syntax == null || thisChanged == true)
            {
                if (newWhenCondition == null && newPattern is ConstantPatternSyntax constantPattern)
                {
                    syntax = RoslynSyntaxFactory.CaseSwitchLabel(constantPattern.Expression);
                }
                else
                {
                    var whenClause = newWhenCondition == null ? null : RoslynSyntaxFactory.WhenClause(newWhenCondition);

                    syntax = RoslynSyntaxFactory.CasePatternSwitchLabel(
                        newPattern, whenClause, RoslynSyntaxFactory.Token(SyntaxKind.ColonToken));
                }

                SetChanged(ref changed);
            }

            return syntax;
        }

        private protected override void SetSyntaxImpl(Roslyn::SyntaxNode newSyntax)
        {
            this.syntax = (CaseSwitchLabelSyntax)newSyntax;

            Set(ref pattern, null);
            Set(ref whenCondition, null);
            whenConditionSet = false;
        }

        private protected override SyntaxNode CloneImpl() => new SwitchCase(Pattern, WhenCondition);

        public override void ReplaceExpressions<T>(Func<T, bool> filter, Func<T, Expression> projection)
        {
            Pattern.ReplaceExpressions(filter, projection);
            WhenCondition = Expression.ReplaceExpressions(WhenCondition, filter, projection);
        }
    }

    public sealed class SwitchDefault : SwitchLabel
    {
        private DefaultSwitchLabelSyntax syntax;

        internal SwitchDefault(DefaultSwitchLabelSyntax syntax, SwitchSection parent)
            : base(syntax)
        {
            this.syntax = syntax;
            Parent = parent;
        }

        public SwitchDefault() { }

        internal override SwitchLabelSyntax GetWrapped(ref bool? changed)
        {
            GetAndResetChanged(ref changed);

            if (syntax == null)
            {
                syntax = RoslynSyntaxFactory.DefaultSwitchLabel();

                SetChanged(ref changed);
            }

            return syntax;
        }

        private protected override void SetSyntaxImpl(Roslyn::SyntaxNode newSyntax) =>
            syntax = (DefaultSwitchLabelSyntax)newSyntax;

        private protected override SyntaxNode CloneImpl() => new SwitchDefault();

        public override void ReplaceExpressions<T>(Func<T, bool> filter, Func<T, Expression> projection) { }
    }
}
