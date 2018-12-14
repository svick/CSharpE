using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RoslynSyntaxFactory = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Roslyn = Microsoft.CodeAnalysis;

namespace CSharpE.Syntax
{
    // TODO: generics
    public sealed class MemberAccessExpression : Expression, ISyntaxWrapper<MemberAccessExpressionSyntax>
    {
        private MemberAccessExpressionSyntax syntax;

        internal MemberAccessExpression(MemberAccessExpressionSyntax syntax, SyntaxNode parent)
        {
            Init(syntax);
            Parent = parent;
        }

        private void Init(MemberAccessExpressionSyntax memberAccessExpressionSyntax)
        {
            syntax = memberAccessExpressionSyntax;

            memberName = new Identifier(memberAccessExpressionSyntax.Name.Identifier);
        }

        public MemberAccessExpression(Expression expression, string memberName)
        {
            Expression = expression;
            MemberName = memberName;
        }

        public MemberAccessExpression(Expression expression, FieldDefinition fieldDefinition)
            : this(expression, fieldDefinition.Name) { }

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

        private Identifier memberName;
        public string MemberName
        {
            get => memberName.Text;
            set => memberName.Text = value;
        }

        
        MemberAccessExpressionSyntax ISyntaxWrapper<MemberAccessExpressionSyntax>.GetWrapped(ref bool? changed)
        {
            GetAndResetChanged(ref changed);

            bool? thisChanged = false;

            var newExpression = expression?.GetWrapped(ref thisChanged) ?? syntax.Expression;
            var newMemberName = memberName.GetWrapped(ref thisChanged);

            if (syntax == null || thisChanged == true)
            {
                syntax = RoslynSyntaxFactory.MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression, newExpression,
                    RoslynSyntaxFactory.IdentifierName(newMemberName));

                SetChanged(ref changed);
            }

            return syntax;
        }

        private protected override ExpressionSyntax GetWrappedExpression(ref bool? changed) =>
            this.GetWrapped<MemberAccessExpressionSyntax>(ref changed);

        private protected override void SetSyntaxImpl(Roslyn::SyntaxNode newSyntax)
        {
            Init((MemberAccessExpressionSyntax)newSyntax);

            Set(ref expression, null);
        }

        internal override SyntaxNode Clone() => new MemberAccessExpression(Expression, MemberName);

        internal override SyntaxNode Parent { get; set; }
    }
}