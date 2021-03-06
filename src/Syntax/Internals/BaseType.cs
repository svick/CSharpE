using Microsoft.CodeAnalysis.CSharp.Syntax;
using RoslynSyntaxFactory = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Roslyn = Microsoft.CodeAnalysis;

namespace CSharpE.Syntax.Internals
{
    internal sealed class BaseType : SyntaxNode, ISyntaxWrapper<BaseTypeSyntax>
    {
        private BaseTypeSyntax syntax;
        
        internal BaseType(BaseTypeSyntax syntax, TypeDefinition parent)
            : base(syntax)
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

        BaseTypeSyntax ISyntaxWrapper<BaseTypeSyntax>.GetWrapped(ref bool? changed)
        {
            GetAndResetChanged(ref changed, out var thisChanged);

            var newType = type?.GetWrapped(ref thisChanged) ?? syntax.Type;

            if (syntax == null || thisChanged == true || ShouldAnnotate(syntax, changed))
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

        private protected override SyntaxNode CloneImpl() => new BaseType(Type);
    }
}