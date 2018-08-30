using System.Collections.Generic;
using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Roslyn = Microsoft.CodeAnalysis;

namespace CSharpE.Syntax
{
    public class DelegateDefinition : BaseTypeDefinition, ISyntaxWrapper<DelegateDeclarationSyntax>
    {
        private DelegateDeclarationSyntax syntax;

        internal DelegateDefinition(DelegateDeclarationSyntax delegateDeclarationSyntax, SyntaxNode parent)
        {
            Init(delegateDeclarationSyntax);

            Parent = parent;
        }

        private void Init(DelegateDeclarationSyntax delegateDeclarationSyntax)
        {
            syntax = delegateDeclarationSyntax;

            name = new Identifier(syntax.Identifier);
            Modifiers = FromRoslyn.MemberModifiers(syntax.Modifiers);
        }

        private TypeReference returnType;
        public TypeReference ReturnType
        {
            get
            {
                if (returnType == null)
                    returnType = FromRoslyn.TypeReference(syntax.ReturnType, this);

                return returnType;
            }
            set => SetNotNull(ref returnType, value);
        }

        private SeparatedSyntaxList<Parameter, ParameterSyntax> parameters;
        public IList<Parameter> Parameters
        {
            get
            {
                if (parameters == null)
                    parameters = new SeparatedSyntaxList<Parameter, ParameterSyntax>(
                        syntax.ParameterList.Parameters, this);

                return parameters;
            }
            set => SetList(ref parameters, new SeparatedSyntaxList<Parameter, ParameterSyntax>(value, this));
        }

        private protected override void SetSyntaxImpl(Roslyn::SyntaxNode newSyntax)
        {
            throw new System.NotImplementedException();
        }

        internal override SyntaxNode Clone()
        {
            throw new System.NotImplementedException();
        }

        private protected override MemberDeclarationSyntax Syntax => syntax;

        private protected override void ValidateModifiers(MemberModifiers modifiers)
        {
            throw new System.NotImplementedException();
        }

        internal DelegateDeclarationSyntax GetWrapped(ref bool? changed)
        {
            throw new System.NotImplementedException();
        }

        private protected override MemberDeclarationSyntax GetWrappedImpl(ref bool? changed) => GetWrapped(ref changed);

        DelegateDeclarationSyntax ISyntaxWrapper<DelegateDeclarationSyntax>.GetWrapped(ref bool? changed) =>
            GetWrapped(ref changed);
    }
}