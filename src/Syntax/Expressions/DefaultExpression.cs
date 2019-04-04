﻿using System;
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
                    var typeSyntax = GetTypeSyntax();

                    if (typeSyntax != null)
                        type = FromRoslyn.TypeReference(typeSyntax, this);

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

            var newType = type?.GetWrapped(ref thisChanged) ?? GetTypeSyntax();

            if (syntax == null || thisChanged == true)
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

        internal override SyntaxNode Clone() => new DefaultExpression(Type);

        internal override SyntaxNode Parent { get; set; }
    }
}