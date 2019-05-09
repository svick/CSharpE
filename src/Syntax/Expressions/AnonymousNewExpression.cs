using System;
using System.Collections.Generic;
using System.Linq;
using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RoslynSyntaxFactory = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Roslyn = Microsoft.CodeAnalysis;

namespace CSharpE.Syntax
{
    public sealed class AnonymousNewExpression : Expression
    {
        private AnonymousObjectCreationExpressionSyntax syntax;

        internal AnonymousNewExpression(AnonymousObjectCreationExpressionSyntax syntax, SyntaxNode parent)
            : base(syntax)
        {
            this.syntax = syntax;
            Parent = parent;
        }

        public AnonymousNewExpression(params AnonymousObjectInitializer[] initializers)
            : this(initializers.AsEnumerable()) { }

        public AnonymousNewExpression(IEnumerable<AnonymousObjectInitializer> initializers) =>
            this.initializers =
                new SeparatedSyntaxList<AnonymousObjectInitializer, AnonymousObjectMemberDeclaratorSyntax>(
                    initializers, this);

        private SeparatedSyntaxList<AnonymousObjectInitializer, AnonymousObjectMemberDeclaratorSyntax> initializers;
        public IList<AnonymousObjectInitializer> Initializers
        {
            get
            {
                if (initializers == null)
                    initializers =
                        new SeparatedSyntaxList<AnonymousObjectInitializer, AnonymousObjectMemberDeclaratorSyntax>(
                            syntax.Initializers, this);

                return initializers;
            }
            set => SetList(ref initializers, new SeparatedSyntaxList<AnonymousObjectInitializer, AnonymousObjectMemberDeclaratorSyntax>(value, this));
        }

        private protected override ExpressionSyntax GetWrappedExpression(ref bool? changed)
        {
            GetAndResetChanged(ref changed);

            bool? thisChanged = false;

            var newInitializers = initializers?.GetWrapped(ref thisChanged) ?? syntax.Initializers;

            if (syntax == null || thisChanged == true || ShouldAnnotate(syntax, changed))
            {
                syntax = RoslynSyntaxFactory.AnonymousObjectCreationExpression(newInitializers);

                syntax = Annotate(syntax);

                SetChanged(ref changed);
            }

            return syntax;
        }

        private protected override void SetSyntaxImpl(Roslyn::SyntaxNode newSyntax)
        {
            syntax = (AnonymousObjectCreationExpressionSyntax)newSyntax;

            SetList(ref initializers, null);
        }

        private protected override SyntaxNode CloneImpl() => new AnonymousNewExpression(Initializers);

        public override IEnumerable<SyntaxNode> GetChildren() => Initializers;

        public override void ReplaceExpressions<T>(Func<T, bool> filter, Func<T, Expression> projection)
        {
            foreach (var initializer in Initializers)
            {
                initializer.Expression = ReplaceExpressions(initializer.Expression, filter, projection);
            }
        }
    }
}