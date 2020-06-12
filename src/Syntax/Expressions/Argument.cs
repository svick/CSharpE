using System;
using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RoslynSyntaxFactory = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Roslyn = Microsoft.CodeAnalysis;

namespace CSharpE.Syntax
{
    public sealed class Argument : SyntaxNode, ISyntaxWrapper<ArgumentSyntax>
    {
        private ArgumentSyntax syntax;

        internal Argument(ArgumentSyntax syntax, SyntaxNode parent)
            : base(syntax)
        {
            Init(syntax);
            Parent = parent;
        }

        private void Init(ArgumentSyntax syntax)
        {
            this.syntax = syntax;

            RefKind = GetSyntaxRefKind();
            name = new Identifier(syntax.NameColon?.Name.Identifier);
        }

        public Argument(Expression expression)
            : this(null, expression) { }

        public Argument(string name, Expression expression)
            : this(name, default, expression) { }

        public Argument(ArgumentRefKind refKind, Expression expression)
            : this(null, refKind, expression) { }

        public Argument(string name, ArgumentRefKind refKind, Expression expression)
        {
            this.name = new Identifier(name, true);
            RefKind = refKind;
            Expression = expression;
        }

        private Identifier name;
        public string Name
        {
            get => name.Text;
            set => name.Text = value;
        }

        public ArgumentRefKind RefKind { get; set; }

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

        private ArgumentRefKind GetSyntaxRefKind() => RefKindMapping[syntax.RefKindKeyword.Kind()];

        internal static readonly BiDirectionalDictionary<ArgumentRefKind, SyntaxKind> RefKindMapping =
            new BiDirectionalDictionary<ArgumentRefKind, SyntaxKind>
            {
                { ArgumentRefKind.None, SyntaxKind.None },
                { ArgumentRefKind.Ref, SyntaxKind.RefKeyword },
                { ArgumentRefKind.Out, SyntaxKind.OutKeyword },
                { ArgumentRefKind.In, SyntaxKind.InKeyword }
            };

        ArgumentSyntax ISyntaxWrapper<ArgumentSyntax>.GetWrapped(ref bool? changed)
        {
            GetAndResetChanged(ref changed, out var thisChanged);

            var newName = name.GetWrapped(ref thisChanged);
            var newExpression = expression?.GetWrapped(ref thisChanged) ?? syntax.Expression;

            if (syntax == null || thisChanged == true || RefKind != GetSyntaxRefKind() || ShouldAnnotate(syntax, changed))
            {
                var refKindKeyword = RoslynSyntaxFactory.Token(RefKindMapping[RefKind]);
                var nameColon = newName == default ? null : RoslynSyntaxFactory.NameColon(RoslynSyntaxFactory.IdentifierName(newName));

                syntax = RoslynSyntaxFactory.Argument(nameColon, refKindKeyword, newExpression);

                syntax = Annotate(syntax);

                SetChanged(ref changed);
            }

            return syntax;
        }

        public static implicit operator Argument(Expression expression) => new Argument(expression);

        private protected override void SetSyntaxImpl(Roslyn::SyntaxNode newSyntax)
        {
            Init((ArgumentSyntax)newSyntax);
            Set(ref expression, null);
        }

        private protected override SyntaxNode CloneImpl() => new Argument(Name, RefKind, Expression);

        public void ReplaceExpressions<T>(Func<T, bool> filter, Func<T, Expression> projection) where T : Expression =>
            Expression = Expression.ReplaceExpressions(Expression, filter, projection);
    }

    public enum ArgumentRefKind
    {
        None,
        Ref,
        Out,
        In
    }
}