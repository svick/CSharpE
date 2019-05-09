using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RoslynSyntaxFactory = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Roslyn = Microsoft.CodeAnalysis;

namespace CSharpE.Syntax
{
    public sealed class CollectionInitializer : Initializer
    {
        private InitializerExpressionSyntax syntax;

        internal CollectionInitializer(InitializerExpressionSyntax syntax, SyntaxNode parent)
            : base(syntax)
        {
            Debug.Assert(syntax.Kind() == SyntaxKind.CollectionInitializerExpression);

            this.syntax = syntax;
            Parent = parent;
        }

        public CollectionInitializer(params ElementInitializer[] elementInitializers)
            : this(elementInitializers.AsEnumerable()) { }

        public CollectionInitializer(IEnumerable<ElementInitializer> elementInitializers)
        {
            this.elementInitializers =
                new SeparatedSyntaxList<ElementInitializer, ExpressionSyntax>(elementInitializers, this);
        }

        private SeparatedSyntaxList<ElementInitializer, ExpressionSyntax> elementInitializers;
        public IList<ElementInitializer> ElementInitializers
        {
            get
            {
                if (elementInitializers == null)
                    elementInitializers =
                        new SeparatedSyntaxList<ElementInitializer, ExpressionSyntax>(syntax.Expressions, this);

                return elementInitializers;
            }
            set => SetList(
                ref elementInitializers,
                new SeparatedSyntaxList<ElementInitializer, ExpressionSyntax>(value, this));
        }

        internal override InitializerExpressionSyntax GetWrapped(ref bool? changed)
        {
            GetAndResetChanged(ref changed);

            bool? thisChanged = false;

            var newElementInitializers = elementInitializers?.GetWrapped(ref thisChanged) ?? syntax.Expressions;

            if (syntax == null || thisChanged == true)
            {
                syntax = RoslynSyntaxFactory.InitializerExpression(
                    SyntaxKind.CollectionInitializerExpression, newElementInitializers);

                SetChanged(ref changed);
            }

            return syntax;
        }

        private protected override void SetSyntaxImpl(Roslyn::SyntaxNode newSyntax)
        {
            SetList(ref elementInitializers, null);
            syntax = (InitializerExpressionSyntax)newSyntax;
        }

        private protected override SyntaxNode CloneImpl() => new CollectionInitializer(ElementInitializers);

        public override IEnumerable<SyntaxNode> GetChildren() => ElementInitializers;

        public override void ReplaceExpressions<T>(Func<T, bool> filter, Func<T, Expression> projection)
        {
            foreach (var initializer in ElementInitializers)
            {
                initializer.ReplaceExpressions(filter, projection);
            }
        }
    }
}
