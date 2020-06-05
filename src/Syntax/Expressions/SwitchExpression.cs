using System.Collections.Generic;
using System.Linq;
using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RoslynSyntaxFactory = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Roslyn = Microsoft.CodeAnalysis;
using System;

namespace CSharpE.Syntax
{
    public sealed class SwitchExpression : Expression
    {
        private SwitchExpressionSyntax syntax;

        internal SwitchExpression(SwitchExpressionSyntax syntax, SyntaxNode parent)
            : base(syntax)
        {
            this.syntax = syntax;
            Parent = parent;
        }

        public SwitchExpression(Expression expression, params SwitchArm[] arms)
            : this(expression, arms.AsEnumerable()) { }

        public SwitchExpression(Expression expression, IEnumerable<SwitchArm> arms)
        {
            Expression = expression;
            Arms = arms?.ToList();
        }

        private Expression expression;
        public Expression Expression
        {
            get
            {
                if (expression == null)
                    expression = FromRoslyn.Expression(syntax.GoverningExpression, this);

                return expression;
            }
            set => SetNotNull(ref expression, value);
        }

        private SeparatedSyntaxList<SwitchArm, SwitchExpressionArmSyntax> arms;
        public IList<SwitchArm> Arms
        {
            get
            {
                if (arms == null)
                    arms = new SeparatedSyntaxList<SwitchArm, SwitchExpressionArmSyntax>(syntax.Arms, this);

                return arms;
            }
            set => SetList(ref arms, new SeparatedSyntaxList<SwitchArm, SwitchExpressionArmSyntax>(value, this));

        }

        private protected override ExpressionSyntax GetWrappedExpression(ref bool? changed)
        {
            GetAndResetChanged(ref changed, out var thisChanged);

            var newExpression = expression?.GetWrapped(ref thisChanged) ?? syntax.GoverningExpression;
            var newArms = arms?.GetWrapped(ref thisChanged) ?? syntax.Arms;

            if (syntax == null || thisChanged == true || ShouldAnnotate(syntax, changed))
            {
                syntax = RoslynSyntaxFactory.SwitchExpression(newExpression, newArms);

                syntax = Annotate(syntax);

                SetChanged(ref changed);
            }

            return syntax;
        }

        private protected override void SetSyntaxImpl(Roslyn::SyntaxNode newSyntax)
        {
            syntax = (SwitchExpressionSyntax)newSyntax;

            Set(ref expression, null);
            SetList(ref arms, null);
        }

        private protected override SyntaxNode CloneImpl() => new SwitchExpression(Expression, Arms);

        public override IEnumerable<SyntaxNode> GetChildren() => new SyntaxNode[] { Expression }.Concat(Arms);

        public override void ReplaceExpressions<T>(Func<T, bool> filter, Func<T, Expression> projection)
        {
            Expression = ReplaceExpressions(Expression, filter, projection);

            foreach (var arm in Arms)
            {
                arm.ReplaceExpressions(filter, projection);
            }
        }
    }
}