using System;
using System.Diagnostics;
using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RoslynSyntaxFactory = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Roslyn = Microsoft.CodeAnalysis;

namespace CSharpE.Syntax
{
    public sealed class DefaultExpression : Expression
    {
        private ExpressionSyntax syntax;

        internal DefaultExpression(ExpressionSyntax syntax, SyntaxNode parent)
            : base(syntax)
        {
            Debug.Assert(
                (syntax is LiteralExpressionSyntax literal && literal.Kind() == SyntaxKind.DefaultLiteralExpression) ||
                syntax is DefaultExpressionSyntax);

            this.syntax = syntax;
            Parent = parent;
        }

        public DefaultExpression(TypeReference type = null) => Type = type;

        private TypeSyntax GetTypeSyntax()
        {
            switch (syntax)
            {
                case LiteralExpressionSyntax _:
                    return null;
                case DefaultExpressionSyntax defaultExpressionSyntax:
                    return defaultExpressionSyntax.Type;
                default:
                    throw new InvalidOperationException();
            }
        }

        private bool typeSet;
        private TypeReference type;
        public TypeReference Type
        {
            get
            {
                if (!typeSet)
                {
                    type = FromRoslyn.TypeReference(GetTypeSyntax(), this);
                    typeSet = true;
                }

                return type;
            }
            set
            {
                Set(ref type, value);
                typeSet = true;
            }
        }

        private protected override ExpressionSyntax GetWrappedExpression(ref bool? changed)
        {
            GetAndResetChanged(ref changed);

            bool? thisChanged = false;

            var newType = typeSet ? type?.GetWrapped(ref thisChanged) : GetTypeSyntax();

            if (syntax == null || thisChanged == true || ShouldAnnotate(syntax, changed))
            {
                if (newType == null)
                {
                    syntax = RoslynSyntaxFactory.LiteralExpression(
                        SyntaxKind.DefaultLiteralExpression, RoslynSyntaxFactory.Token(SyntaxKind.DefaultKeyword));
                }
                else
                {
                    syntax = RoslynSyntaxFactory.DefaultExpression(newType);
                }

                syntax = Annotate(syntax);

                SetChanged(ref changed);
            }

            return syntax;
        }

        private protected override void SetSyntaxImpl(Roslyn::SyntaxNode newSyntax)
        {
            syntax = (ObjectCreationExpressionSyntax)newSyntax;

            Set(ref type, null);
            typeSet = false;
        }

        private protected override SyntaxNode CloneImpl() => new DefaultExpression(Type);
    }
}