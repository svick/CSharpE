using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RoslynSyntaxFactory = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Roslyn = Microsoft.CodeAnalysis;

namespace CSharpE.Syntax
{
    public sealed class DeclarationExpression : Expression
    {
        private DeclarationExpressionSyntax syntax;

        internal DeclarationExpression(DeclarationExpressionSyntax syntax, SyntaxNode parent)
            : base(syntax)
        {
            this.syntax = syntax;
            Parent = parent;
        }

        public DeclarationExpression(TypeReference type, VariableDesignation designation)
        {
            Type = type;
            Designation = designation;
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

        private VariableDesignation designation;
        public VariableDesignation Designation
        {
            get
            {
                if (designation == null)
                    designation = FromRoslyn.VariableDesignation(syntax.Designation, this);

                return designation;
            }
            set => SetNotNull(ref designation, value);
        }

        private protected override ExpressionSyntax GetWrappedExpression(ref bool? changed)
        {
            GetAndResetChanged(ref changed, out var thisChanged);

            var newType = type?.GetWrapped(ref thisChanged) ?? syntax.Type;
            var newDesignation = designation?.GetWrapped(ref thisChanged) ?? syntax.Designation;

            if (syntax == null || thisChanged == true || ShouldAnnotate(syntax, changed))
            {
                syntax = RoslynSyntaxFactory.DeclarationExpression(newType, newDesignation);

                syntax = Annotate(syntax);

                SetChanged(ref changed);
            }

            return syntax;
        }

        private protected override void SetSyntaxImpl(Roslyn::SyntaxNode newSyntax)
        {
            syntax = (DeclarationExpressionSyntax)newSyntax;

            Set(ref type, null);
            Set(ref designation, null);
        }

        private protected override SyntaxNode CloneImpl() => new DeclarationExpression(Type, Designation);
    }
}