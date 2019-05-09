using System;
using System.Collections.Generic;
using System.Linq;
using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static CSharpE.Syntax.MemberModifiers;
using RoslynSyntaxFactory = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Roslyn = Microsoft.CodeAnalysis;

namespace CSharpE.Syntax
{
    // TODO: generics
    public sealed class DelegateDefinition : BaseTypeDefinition, ISyntaxWrapper<DelegateDeclarationSyntax>
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

        public DelegateDefinition(TypeReference returnType, params Parameter[] parameters)
            : this(returnType, parameters.AsEnumerable()) { }

        public DelegateDefinition(TypeReference returnType, IEnumerable<Parameter> parameters)
        {
            ReturnType = returnType;
            this.parameters = new SeparatedSyntaxList<Parameter, ParameterSyntax>(parameters, this);
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
            Init((DelegateDeclarationSyntax)newSyntax);

            Set(ref returnType, null);
            SetList(ref parameters, null);
        }

        private protected override SyntaxNode CloneImpl() => new DelegateDefinition(ReturnType, Parameters);

        private protected override MemberDeclarationSyntax MemberSyntax => syntax;

        private const MemberModifiers ValidModifiers = AccessModifiersMask | New | Unsafe;
        private protected override void ValidateModifiers(MemberModifiers value)
        {
            var invalidModifiers = value & ~ValidModifiers;
            if (invalidModifiers != 0)
                throw new ArgumentException($"The modifiers {invalidModifiers} are not valid for a delegate.", nameof(value));
        }

        DelegateDeclarationSyntax ISyntaxWrapper<DelegateDeclarationSyntax>.GetWrapped(ref bool? changed)
        {
            GetAndResetChanged(ref changed);

            bool? thisChanged = false;

            var newAttributes = attributes?.GetWrapped(ref thisChanged) ?? syntax?.AttributeLists ?? default;
            var newModifiers = Modifiers;
            var newName = name.GetWrapped(ref thisChanged);
            var newReturnType = returnType?.GetWrapped(ref thisChanged) ?? syntax.ReturnType;
            var newParameters = parameters?.GetWrapped(ref thisChanged) ?? syntax.ParameterList.Parameters;

            if (syntax == null || FromRoslyn.MemberModifiers(syntax.Modifiers) != newModifiers ||
                thisChanged == true || ShouldAnnotate(syntax, changed))
            {
                var newSyntax = RoslynSyntaxFactory.DelegateDeclaration(
                    newAttributes, newModifiers.GetWrapped(), newReturnType, newName, default,
                    RoslynSyntaxFactory.ParameterList(newParameters), default);

                syntax = Annotate(newSyntax);

                SetChanged(ref changed);
            }

            return syntax;
        }

        private protected override MemberDeclarationSyntax GetWrappedMember(ref bool? changed) =>
            this.GetWrapped<DelegateDeclarationSyntax>(ref changed);

        protected override void ReplaceExpressionsImpl<T>(Func<T, bool> filter, Func<T, Expression> projection) { }
    }
}