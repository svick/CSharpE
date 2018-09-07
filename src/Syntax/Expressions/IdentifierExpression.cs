using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using CSharpSyntaxFactory = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Roslyn = Microsoft.CodeAnalysis;

namespace CSharpE.Syntax
{
    public sealed class IdentifierExpression : Expression, ISyntaxWrapper<IdentifierNameSyntax>
    {
        private IdentifierNameSyntax syntax;
        
        public IdentifierExpression(IdentifierNameSyntax syntax, SyntaxNode parent)
        {
            this.syntax = syntax;
            Parent = parent;
        }

        private void Init(IdentifierNameSyntax syntax)
        {
            this.syntax = syntax;
            identifier = new Identifier(syntax.Identifier);
        }

        public IdentifierExpression(string identifier) => Identifier = identifier;

        private Identifier identifier;
        public string Identifier
        {
            get => identifier.Text;
            set => identifier.Text = value;
        }
        
        private protected override ExpressionSyntax GetWrappedExpression(ref bool? changed) =>
            this.GetWrapped<IdentifierNameSyntax>();

        IdentifierNameSyntax ISyntaxWrapper<IdentifierNameSyntax>.GetWrapped(ref bool? changed)
        {
            GetAndResetChanged(ref changed);

            bool? thisChanged = false;

            var newIdentifier = identifier.GetWrapped(ref thisChanged);

            if (syntax == null || thisChanged == true)
            {
                syntax = CSharpSyntaxFactory.IdentifierName(newIdentifier);

                SetChanged(ref changed);
            }

            return syntax;

        }

        private protected override void SetSyntaxImpl(Roslyn::SyntaxNode newSyntax)
        {
            throw new System.NotImplementedException();
        }

        internal override SyntaxNode Clone() => new IdentifierExpression(Identifier);

        internal override SyntaxNode Parent { get; set; }
    }
}