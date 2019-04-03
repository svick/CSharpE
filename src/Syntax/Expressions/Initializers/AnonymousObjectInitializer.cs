using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RoslynSyntaxFactory = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Roslyn = Microsoft.CodeAnalysis;

namespace CSharpE.Syntax
{
    public sealed class AnonymousObjectInitializer : SyntaxNode, ISyntaxWrapper<AnonymousObjectMemberDeclaratorSyntax>
    {
        private AnonymousObjectMemberDeclaratorSyntax syntax;

        internal AnonymousObjectInitializer(AnonymousObjectMemberDeclaratorSyntax syntax, AnonymousNewExpression parent)
        {
            Init(syntax);
            Parent = parent;
        }

        private void Init(AnonymousObjectMemberDeclaratorSyntax syntax)
        {
            this.syntax = syntax;
            name = new Identifier(syntax.NameEquals?.Name.Identifier ?? default, true);
        }

        public AnonymousObjectInitializer(Expression expression) : this(null, expression) { }

        public AnonymousObjectInitializer(string name, Expression expression)
        {
            this.name = new Identifier(name, true);
            Expression = expression;
        }

        private Identifier name;
        public string Name
        {
            get => name.Text;
            set => name.Text = value;
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
            set => Set(ref expression, value);
        }

        AnonymousObjectMemberDeclaratorSyntax ISyntaxWrapper<AnonymousObjectMemberDeclaratorSyntax>.GetWrapped(ref bool? changed)
        {
            GetAndResetChanged(ref changed);

            bool? thisChanged = false;

            var newName = name.GetWrapped(ref thisChanged);
            var newExpression = expression?.GetWrapped(ref thisChanged) ?? syntax.Expression;

            if (syntax == null || thisChanged == true)
            {
                var nameEquals = newName == default
                    ? null
                    : RoslynSyntaxFactory.NameEquals(RoslynSyntaxFactory.IdentifierName(newName));

                syntax = RoslynSyntaxFactory.AnonymousObjectMemberDeclarator(nameEquals, newExpression);

                SetChanged(ref changed);
            }

            return syntax;
        }

        internal override SyntaxNode Parent { get; set; }

        private protected override void SetSyntaxImpl(Roslyn::SyntaxNode newSyntax)
        {
            Set(ref expression, null);
            Init((AnonymousObjectMemberDeclaratorSyntax)newSyntax);
        }

        internal override SyntaxNode Clone() => new AnonymousObjectInitializer(Name, Expression);
    }
}