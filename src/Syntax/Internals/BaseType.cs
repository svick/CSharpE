using Microsoft.CodeAnalysis.CSharp.Syntax;
using RoslynSyntaxFactory = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Roslyn = Microsoft.CodeAnalysis;

namespace CSharpE.Syntax.Internals
{
    internal sealed class BaseType : SyntaxNode, ISyntaxWrapper<BaseTypeSyntax>
    {
        private BaseTypeSyntax syntax;
        
        internal BaseType(BaseTypeSyntax syntax, TypeDefinition parent)
        {
            this.syntax = syntax;
            this.Parent = parent;
        }

        public BaseType(TypeReference type) => Set(ref this.type, type);

        private TypeReference type;
        public TypeReference Type
        {
            get
            {
                if (type == null)
                    type = new NamedTypeReference(syntax.Type, this);

                return type;
            }
        }

        internal override SyntaxNode Parent { get; set; }

        BaseTypeSyntax ISyntaxWrapper<BaseTypeSyntax>.GetWrapped(ref bool? changed)
        {
            GetAndResetChanged(ref changed);
            
            bool? thisChanged = false;

            var newType = type?.GetWrapped(ref thisChanged) ?? syntax.Type;

            if (syntax == null || thisChanged == true || !IsAnnotated(syntax))
            {
                var newSyntax = RoslynSyntaxFactory.SimpleBaseType(newType);

                syntax = Annotate(newSyntax);
                
                SetChanged(ref changed);
            }

            return syntax;
        }
        
        private protected override void SetSyntaxImpl(Roslyn::SyntaxNode newSyntax)
        {
            syntax = (BaseTypeSyntax)newSyntax;
            Set(ref type, null);
        }

        internal override SyntaxNode Clone() => new BaseType(Type);
    }
}