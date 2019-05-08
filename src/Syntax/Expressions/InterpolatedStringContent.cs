using System;
using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RoslynSyntaxFactory = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Roslyn = Microsoft.CodeAnalysis;

namespace CSharpE.Syntax
{
    public abstract class InterpolatedStringContent : SyntaxNode, ISyntaxWrapper<InterpolatedStringContentSyntax>
    {

        InterpolatedStringContentSyntax ISyntaxWrapper<InterpolatedStringContentSyntax>.GetWrapped(ref bool? changed) =>
            GetWrapped(ref changed);

        internal abstract InterpolatedStringContentSyntax GetWrapped(ref bool? changed);

        public abstract void ReplaceExpressions<T>(Func<T, bool> filter, Func<T, Expression> projection)
            where T : Expression;

        public static implicit operator InterpolatedStringContent(string text) => new InterpolatedStringText(text);
    }

    public sealed class Interpolation : InterpolatedStringContent
    {
        private InterpolationSyntax syntax;

        internal Interpolation(InterpolationSyntax syntax, InterpolatedStringExpression parent)
        {
            Init(syntax);
            Parent = parent;
        }

        private void Init(InterpolationSyntax syntax)
        {
            this.syntax = syntax;
            Format = GetSyntaxFormat();
        }

        public Interpolation(Expression expression, Expression alignment = null, string format = null)
        {
            Expression = expression;
            Alignment = alignment;
            Format = format;
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

        private bool alignmentSet;
        private Expression alignment;
        public Expression Alignment
        {
            get
            {
                if (!alignmentSet)
                {
                    alignment = FromRoslyn.Expression(syntax.AlignmentClause?.Value, this);
                    alignmentSet = true;
                }

                return alignment;
            }
            set
            {
                Set(ref alignment, value);
                alignmentSet = true;
            }
        }

        private string GetSyntaxFormat() => syntax.FormatClause?.FormatStringToken.ValueText;

        public string Format { get; set; }

        internal override InterpolatedStringContentSyntax GetWrapped(ref bool? changed)
        {
            GetAndResetChanged(ref changed);

            bool? thisChanged = false;

            var newExpression = expression?.GetWrapped(ref thisChanged) ?? syntax.Expression;
            var newAlignment = alignmentSet ? alignment?.GetWrapped(ref thisChanged) : syntax.AlignmentClause?.Value;

            if (syntax == null || thisChanged == true || Format != GetSyntaxFormat())
            {
                var alignmentClause = newAlignment == null
                    ? null
                    : RoslynSyntaxFactory.InterpolationAlignmentClause(
                        RoslynSyntaxFactory.Token(SyntaxKind.CommaToken), newAlignment);

                var formatClause = Format == null
                    ? null
                    : RoslynSyntaxFactory.InterpolationFormatClause(
                        RoslynSyntaxFactory.Token(SyntaxKind.ColonToken),
                        RoslynSyntaxFactory.Token(
                            default, SyntaxKind.InterpolatedStringTextToken, Format, Format, default));

                syntax = RoslynSyntaxFactory.Interpolation(newExpression, alignmentClause, formatClause);

                SetChanged(ref changed);
            }

            return syntax;
        }

        private protected override void SetSyntaxImpl(Roslyn::SyntaxNode newSyntax)
        {
            Set(ref expression, null);
            Init((InterpolationSyntax)newSyntax);
        }

        private protected override SyntaxNode CloneImpl() => new Interpolation(Expression, Alignment, Format);

        public override void ReplaceExpressions<T>(Func<T, bool> filter, Func<T, Expression> projection) =>
            Expression = Expression.ReplaceExpressions(Expression, filter, projection);
    }

    public sealed class InterpolatedStringText : InterpolatedStringContent
    {
        private InterpolatedStringTextSyntax syntax;

        internal InterpolatedStringText(InterpolatedStringTextSyntax syntax, InterpolatedStringExpression parent)
        {
            Init(syntax);
            Parent = parent;
        }

        private void Init(InterpolatedStringTextSyntax syntax)
        {
            this.syntax = syntax;
            Text = syntax.TextToken.ValueText;
        }

        public InterpolatedStringText(string text) => Text = text;

        private string text;
        public string Text
        {
            get => text;
            set => text = value ?? throw new ArgumentNullException(nameof(value));
        }

        internal override InterpolatedStringContentSyntax GetWrapped(ref bool? changed)
        {
            GetAndResetChanged(ref changed);

            if (syntax == null || Text != syntax.TextToken.ValueText)
            {
                syntax = RoslynSyntaxFactory.InterpolatedStringText(
                    RoslynSyntaxFactory.Token(
                        default, SyntaxKind.InterpolatedStringTextToken, Text, Text, default));

                SetChanged(ref changed);
            }

            return syntax;
        }

        private protected override void SetSyntaxImpl(Roslyn::SyntaxNode newSyntax) =>
            Init((InterpolatedStringTextSyntax)newSyntax);

        private protected override SyntaxNode CloneImpl() => new InterpolatedStringText(Text);

        public override void ReplaceExpressions<T>(Func<T, bool> filter, Func<T, Expression> projection) { }
    }
}