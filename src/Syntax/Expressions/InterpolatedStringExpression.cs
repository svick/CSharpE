using System;
using System.Collections.Generic;
using System.Linq;
using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RoslynSyntaxFactory = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Roslyn = Microsoft.CodeAnalysis;

namespace CSharpE.Syntax
{
    public sealed class InterpolatedStringExpression : Expression
    {
        private InterpolatedStringExpressionSyntax syntax;

        internal InterpolatedStringExpression(InterpolatedStringExpressionSyntax syntax, SyntaxNode parent)
        {
            Init(syntax);
            Parent = parent;
        }

        private void Init(InterpolatedStringExpressionSyntax syntax)
        {
            this.syntax = syntax;
            IsVerbatim = IsSyntaxVerbatim();
        }

        public InterpolatedStringExpression(params InterpolatedStringContent[] contents)
            : this(contents.AsEnumerable()) { }

        public InterpolatedStringExpression(IEnumerable<InterpolatedStringContent> contents)
            : this(false, contents) { }

        public InterpolatedStringExpression(bool isVerbatim, params InterpolatedStringContent[] contents)
            : this(isVerbatim, contents.AsEnumerable()) { }

        public InterpolatedStringExpression(bool isVerbatim, IEnumerable<InterpolatedStringContent> contents)
        {
            IsVerbatim = isVerbatim;
            this.contents = new InterpolatedStringContentList(contents, this);
        }

        private bool IsSyntaxVerbatim() =>
            syntax.StringStartToken.Kind() == SyntaxKind.InterpolatedVerbatimStringStartToken;

        public bool IsVerbatim { get; set; }

        private InterpolatedStringContentList contents;
        public IList<InterpolatedStringContent> Contents
        {
            get
            {
                if (contents == null)
                    contents = new InterpolatedStringContentList(syntax.Contents, this);

                return contents;
            }
            set => SetList(ref contents, new InterpolatedStringContentList(value, this));
        }

        private protected override ExpressionSyntax GetWrappedExpression(ref bool? changed)
        {
            GetAndResetChanged(ref changed);

            bool? thisChanged = false;

            var newContents = contents?.GetWrapped(ref thisChanged) ?? syntax.Contents;

            if (syntax == null || thisChanged == true || IsVerbatim != IsSyntaxVerbatim() || ShouldAnnotate(syntax, changed))
            {
                var startTokenKind = IsVerbatim
                    ? SyntaxKind.InterpolatedVerbatimStringStartToken
                    : SyntaxKind.InterpolatedStringStartToken;

                syntax = RoslynSyntaxFactory.InterpolatedStringExpression(
                    RoslynSyntaxFactory.Token(startTokenKind), newContents);

                syntax = Annotate(syntax);

                SetChanged(ref changed);
            }

            return syntax;
        }

        private protected override void SetSyntaxImpl(Roslyn::SyntaxNode newSyntax)
        {
            syntax = (InterpolatedStringExpressionSyntax)newSyntax;

            SetList(ref contents, null);
        }

        private protected override SyntaxNode CloneImpl() => new InterpolatedStringExpression(Contents);

        public override IEnumerable<SyntaxNode> GetChildren() => Contents;

        public override void ReplaceExpressions<T>(Func<T, bool> filter, Func<T, Expression> projection)
        {
            foreach (var content in Contents)
            {
                content.ReplaceExpressions(filter, projection);
            }
        }
    }
}