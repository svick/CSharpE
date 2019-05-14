using System;
using CSharpE.Syntax.Internals;
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
        private CaseSwitchLabelSyntax syntax;

        internal SwitchCase(CaseSwitchLabelSyntax syntax, SwitchSection parent)
            : base(syntax)
        {
            this.syntax = syntax;
            Parent = parent;
        }

        public SwitchCase(Expression value) => Value = value;

        private Expression value;
        public Expression Value
        {
            get
            {
                if (value == null)
                    value = FromRoslyn.Expression(syntax.Value, this);

                return value;
            }
            set => SetNotNull(ref this.value, value);
        }

        internal override SwitchLabelSyntax GetWrapped(ref bool? changed)
        {
            GetAndResetChanged(ref changed);

            bool? thisChanged = false;

            var newValue = value?.GetWrapped(ref thisChanged) ?? syntax.Value;

            if (syntax == null || thisChanged == true)
            {
                syntax = RoslynSyntaxFactory.CaseSwitchLabel(newValue);

                SetChanged(ref changed);
            }

            return syntax;
        }

        private protected override void SetSyntaxImpl(Roslyn::SyntaxNode newSyntax)
        {
            this.syntax = (CaseSwitchLabelSyntax)newSyntax;

            Set(ref value, null);
        }

        private protected override SyntaxNode CloneImpl() => new SwitchCase(Value);

        public override void ReplaceExpressions<T>(Func<T, bool> filter, Func<T, Expression> projection) =>
            Value = Expression.ReplaceExpressions(Value, filter, projection);
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
