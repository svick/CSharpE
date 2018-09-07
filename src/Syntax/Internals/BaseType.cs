using System;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using CSharpSyntaxFactory = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Roslyn = Microsoft.CodeAnalysis;

namespace CSharpE.Syntax.Internals
{
    internal sealed class BaseType : SyntaxNode, ISyntaxWrapper<BaseTypeSyntax>
    {
        private BaseTypeSyntax syntax;
        
        internal BaseType(BaseTypeSyntax syntax, TypeDefinition parent)
        {
            this.syntax = syntax;
            this.parent = parent;
        }

        public BaseType(TypeReference type, TypeDefinition parent)
        {
            Set(ref this.type, type);
            this.parent = parent;
        }

        private TypeReference type;
        public TypeReference Type
        {
            get
            {
                if (type == null)
                {
                    type = new NamedTypeReference(syntax.Type, this);
                }

                return type;
            }
        }

        private TypeDefinition parent;
        internal override SyntaxNode Parent
        {
            get => parent;
            set
            {
                if (value is TypeDefinition typeDefinition)
                    parent = typeDefinition;
                else
                    throw new ArgumentException(nameof(value));
            }
        }

        BaseTypeSyntax ISyntaxWrapper<BaseTypeSyntax>.GetWrapped(ref bool? changed)
        {
            GetAndResetChanged(ref changed);
            
            bool? thisChanged = false;

            var newType = type?.GetWrapped(ref thisChanged) ?? syntax.Type;

            if (syntax == null || thisChanged == true || !IsAnnotated(syntax))
            {
                var newSyntax = CSharpSyntaxFactory.SimpleBaseType(newType);

                syntax = Annotate(newSyntax);
                
                SetChanged(ref changed);
            }

            return syntax;
        }
        
        private protected override void SetSyntaxImpl(Roslyn::SyntaxNode newSyntax)
        {
            throw new NotImplementedException();
        }

        internal override SyntaxNode Clone()
        {
            throw new NotImplementedException();
        }
    }
}