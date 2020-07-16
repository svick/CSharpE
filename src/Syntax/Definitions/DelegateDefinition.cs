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
    public sealed class DelegateDefinition : BaseTypeDefinition, ISyntaxWrapper<DelegateDeclarationSyntax>
    {
        private DelegateDeclarationSyntax syntax;

        internal DelegateDefinition(DelegateDeclarationSyntax syntax, SyntaxNode parent)
            : base(syntax)
        {
            Init(syntax);

            Parent = parent;
        }

        private void Init(DelegateDeclarationSyntax syntax)
        {
            this.syntax = syntax;

            name = new Identifier(this.syntax.Identifier);
            Modifiers = FromRoslyn.MemberModifiers(this.syntax.Modifiers);
        }

        public DelegateDefinition(TypeReference returnType, string name, params Parameter[] parameters)
            : this(returnType, name, parameters.AsEnumerable()) { }

        public DelegateDefinition(TypeReference returnType, string name, IEnumerable<Parameter> parameters)
            : this(returnType, name, null, parameters) { }

        public DelegateDefinition(
            TypeReference returnType, string name, IEnumerable<TypeParameter> typeParameters, IEnumerable<Parameter> parameters,
            IEnumerable<TypeParameterConstraintClause> constraintClauses = null)
        {
            ReturnType = returnType;
            Name = name;
            TypeParameters = typeParameters?.ToList();
            Parameters = parameters?.ToList();
            ConstraintClauses = constraintClauses?.ToList();
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

        private SeparatedSyntaxList<TypeParameter, TypeParameterSyntax> typeParameters;
        public IList<TypeParameter> TypeParameters
        {
            get
            {
                if (typeParameters == null)
                    typeParameters = new SeparatedSyntaxList<TypeParameter, TypeParameterSyntax>(
                        syntax.TypeParameterList?.Parameters ?? default, this);

                return typeParameters;
            }
            set => SetList(ref typeParameters, new SeparatedSyntaxList<TypeParameter, TypeParameterSyntax>(value, this));
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

        private SyntaxList<TypeParameterConstraintClause, TypeParameterConstraintClauseSyntax> constraintClauses;
        public IList<TypeParameterConstraintClause> ConstraintClauses
        {
            get
            {
                if (constraintClauses == null)
                    constraintClauses = new SyntaxList<TypeParameterConstraintClause, TypeParameterConstraintClauseSyntax>(
                        syntax.ConstraintClauses, this);

                return constraintClauses;
            }
            set => SetList(
                ref constraintClauses,
                new SyntaxList<TypeParameterConstraintClause, TypeParameterConstraintClauseSyntax>(value, this));
        }

        private protected override void SetSyntaxImpl(Roslyn::SyntaxNode newSyntax)
        {
            Init((DelegateDeclarationSyntax)newSyntax);

            Set(ref returnType, null);
            SetList(ref typeParameters, null);
            SetList(ref parameters, null);
            SetList(ref constraintClauses, null);
        }

        private protected override SyntaxNode CloneImpl() =>
            new DelegateDefinition(ReturnType, Name, TypeParameters, Parameters, ConstraintClauses);

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
            GetAndResetChanged(ref changed, out var thisChanged);

            var newAttributes = attributes?.GetWrapped(ref thisChanged) ?? syntax?.AttributeLists ?? default;
            var newModifiers = Modifiers;
            var newName = name.GetWrapped(ref thisChanged);
            var newReturnType = returnType?.GetWrapped(ref thisChanged) ?? syntax.ReturnType;
            var newTypeParameters = typeParameters?.GetWrapped(ref thisChanged) ?? syntax.TypeParameterList?.Parameters ?? default;
            var newParameters = parameters?.GetWrapped(ref thisChanged) ?? syntax.ParameterList.Parameters;
            var newConstraints = constraintClauses?.GetWrapped(ref thisChanged) ?? syntax.ConstraintClauses;

            if (syntax == null || FromRoslyn.MemberModifiers(syntax.Modifiers) != newModifiers ||
                thisChanged == true || ShouldAnnotate(syntax, changed))
            {
                var typeParameterList = newTypeParameters.Any() ? RoslynSyntaxFactory.TypeParameterList(newTypeParameters) : default;

                var newSyntax = RoslynSyntaxFactory.DelegateDeclaration(
                    newAttributes, newModifiers.GetWrapped(), newReturnType, newName, typeParameterList,
                    RoslynSyntaxFactory.ParameterList(newParameters), newConstraints);

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