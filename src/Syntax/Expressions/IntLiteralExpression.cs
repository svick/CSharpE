﻿using System;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxKind;
using RoslynSyntaxFactory = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Roslyn = Microsoft.CodeAnalysis;

namespace CSharpE.Syntax
{
    public sealed class IntLiteralExpression : LiteralExpression
    {
        internal IntLiteralExpression(LiteralExpressionSyntax syntax, SyntaxNode parent)
            : base(syntax)
        {
            Init(syntax);

            Parent = parent;
        }

        private void Init(LiteralExpressionSyntax syntax)
        {
            Syntax = syntax;
            Value = syntax.Token.Value as int? ?? throw new ArgumentException(nameof(syntax));
        }

        public IntLiteralExpression(int value) => Value = value;

        public new int Value { get; set; }

        protected override object ValueImpl => Value;

        private protected override LiteralExpressionSyntax GetWrappedLiteral(ref bool? changed)
        {
            GetAndResetChanged(ref changed);

            if (Syntax == null || Value != (int)Syntax.Token.Value || ShouldAnnotate(Syntax, changed))
            {
                Syntax = RoslynSyntaxFactory.LiteralExpression(
                    NumericLiteralExpression, RoslynSyntaxFactory.Literal(Value));

                Syntax = Annotate(Syntax);

                SetChanged(ref changed);
            }

            return Syntax;
        }

        private protected override void SetSyntaxImpl(Roslyn::SyntaxNode newSyntax) => Init((LiteralExpressionSyntax)newSyntax);

        private protected override SyntaxNode CloneImpl() => new IntLiteralExpression(Value);
    }
}