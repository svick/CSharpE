using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RoslynSyntaxFactory = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Roslyn = Microsoft.CodeAnalysis;

namespace CSharpE.Syntax
{
    public sealed class CastExpression : Expression
    {
        private CastExpressionSyntax syntax;

        internal CastExpression(CastExpressionSyntax syntax, SyntaxNode parent)
            : base(syntax)
        {
            this.syntax = syntax;
            Parent = parent;
        }

        public CastExpression(TypeReference type, Expression expression)
        {
            Type = type;
            Expression = expression;
        }

        private TypeReference type;
        public TypeReference Type
        {
            get
            {
                if (type == null)
                    type = FromRoslyn.TypeReference(syntax.Type, this);

                return type;
            }
            set => SetNotNull(ref type, value);
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

        private protected override ExpressionSyntax GetWrappedExpression(ref bool? changed)
        {
            GetAndResetChanged(ref changed);

            bool? thisChanged = false;

            var newType = type?.GetWrapped(ref thisChanged) ?? syntax.Type;
            var newExpression = expression?.GetWrapped(ref thisChanged) ?? syntax.Expression;

            if (syntax == null || thisChanged == true || ShouldAnnotate(syntax, changed))
            {
                syntax = RoslynSyntaxFactory.CastExpression(newType, newExpression);

                syntax = Annotate(syntax);

                SetChanged(ref changed);
            }

            return syntax;
        }

        private protected override void SetSyntaxImpl(Roslyn::SyntaxNode newSyntax)
        {
            Set(ref type, null);
            Set(ref expression, null);
            syntax = (CastExpressionSyntax)newSyntax;
        }

        private protected override SyntaxNode CloneImpl() => new CastExpression(Type, Expression);
    }
}