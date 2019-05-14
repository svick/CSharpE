using System;
using System.Diagnostics;
using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RoslynSyntaxFactory = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Roslyn = Microsoft.CodeAnalysis;

namespace CSharpE.Syntax
{
    public sealed class GotoCaseStatement : Statement
    {
        private GotoStatementSyntax syntax;

        internal GotoCaseStatement(GotoStatementSyntax syntax, SyntaxNode parent)
            : base(syntax)
        {
            Debug.Assert(syntax.Kind() == SyntaxKind.GotoCaseStatement);

            this.syntax = syntax;
            Parent = parent;
        }

        internal GotoCaseStatement(Expression value) => Value = value;

        private Expression value;
        public Expression Value
        {
            get
            {
                if (value == null)
                    value = FromRoslyn.Expression(syntax.Expression, this);

                return value;
            }
            set => SetNotNull(ref this.value, value);
        }

        private protected override StatementSyntax GetWrappedStatement(ref bool? changed)
        {
            GetAndResetChanged(ref changed);

            bool? thisChanged = false;

            var newValue = value?.GetWrapped(ref thisChanged) ?? syntax.Expression;

            if (syntax == null || thisChanged == true)
            {
                syntax = RoslynSyntaxFactory.GotoStatement(
                    SyntaxKind.GotoCaseStatement, RoslynSyntaxFactory.Token(SyntaxKind.CaseKeyword), newValue);

                SetChanged(ref changed);
            }

            return syntax;
        }

        private protected override void SetSyntaxImpl(Roslyn::SyntaxNode newSyntax) =>
            this.syntax = (GotoStatementSyntax)newSyntax;

        private protected override SyntaxNode CloneImpl() => new GotoCaseStatement(Value);

        public override void ReplaceExpressions<T>(Func<T, bool> filter, Func<T, Expression> projection) =>
            Value = Expression.ReplaceExpressions(Value, filter, projection);
    }
}