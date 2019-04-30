using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RoslynSyntaxFactory = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Roslyn = Microsoft.CodeAnalysis;

namespace CSharpE.Syntax
{
    public sealed class TypeOfExpression : Expression
    {
        private TypeOfExpressionSyntax syntax;

        internal TypeOfExpression(TypeOfExpressionSyntax syntax, SyntaxNode parent)
        {
            this.syntax = syntax;
            Parent = parent;
        }

        public TypeOfExpression(TypeReference type) => Type = type;

        private TypeReference type;
        public TypeReference Type
        {
            get
            {
                if (type == null)
                {
                    type = FromRoslyn.TypeReference(syntax.Type, this);
                }

                return type;
            }
            set => SetNotNull(ref type, value);
        }

        private protected override ExpressionSyntax GetWrappedExpression(ref bool? changed)
        {
            GetAndResetChanged(ref changed);

            bool? thisChanged = false;

            var newType = type?.GetWrapped(ref thisChanged) ?? syntax.Type;

            if (syntax == null || thisChanged == true)
            {
                syntax = RoslynSyntaxFactory.TypeOfExpression(newType);

                SetChanged(ref changed);
            }

            return syntax;
        }

        private protected override void SetSyntaxImpl(Roslyn::SyntaxNode newSyntax)
        {
            syntax = (TypeOfExpressionSyntax)newSyntax;

            Set(ref type, null);
        }

        private protected override SyntaxNode CloneImpl() => new TypeOfExpression(Type);

        internal override SyntaxNode Parent { get; set; }
    }
}