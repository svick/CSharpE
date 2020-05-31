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
    public sealed class MethodDefinition : BaseMethodDefinition, ISyntaxWrapper<MethodDeclarationSyntax>
    {
        private MethodDeclarationSyntax syntax;

        private protected override BaseMethodDeclarationSyntax BaseMethodSyntax => syntax;

        internal MethodDefinition(MethodDeclarationSyntax syntax, TypeDefinition parent)
            : base(syntax)
        {
            Init(syntax);
            Parent = parent;
        }

        private void Init(MethodDeclarationSyntax methodDeclarationSyntax)
        {
            syntax = methodDeclarationSyntax;

            Modifiers = FromRoslyn.MemberModifiers(methodDeclarationSyntax.Modifiers);
            name = new Identifier(methodDeclarationSyntax.Identifier);
        }

        public MethodDefinition(
            MemberModifiers modifiers, TypeReference returnType, string name, IEnumerable<Parameter> parameters,
            IEnumerable<Statement> body)
            : this(modifiers, returnType, null, name, parameters, body) { }

        public MethodDefinition(
            MemberModifiers modifiers, TypeReference returnType, NamedTypeReference explicitInterface, string name,
            IEnumerable<Parameter> parameters, IEnumerable<Statement> body)
            : this(modifiers, returnType, explicitInterface, name, null, parameters, null, body) { }

        public MethodDefinition(
            MemberModifiers modifiers, TypeReference returnType, NamedTypeReference explicitInterface, string name,
            IEnumerable<TypeParameter> typeParameters, IEnumerable<Parameter> parameters,
            IEnumerable<TypeParameterConstraintClause> constraintClauses, IEnumerable<Statement> body)
        {
            Modifiers = modifiers;
            ReturnType = returnType;
            ExplicitInterface = explicitInterface;
            Name = name;
            TypeParameters = typeParameters?.ToList();
            Parameters = parameters?.ToList();
            ConstraintClauses = constraintClauses?.ToList();
            Body = new BlockStatement(body);
        }

        #region Modifiers

        private const MemberModifiers ValidMethodModifiers =
            AccessModifiersMask | New | Static | Unsafe | Abstract | Sealed | Virtual | Override | Extern | Partial |
            Async;

        private protected override void ValidateModifiers(MemberModifiers value)
        {
            var invalidModifiers = value & ~ValidMethodModifiers;
            if (invalidModifiers != 0)
                throw new ArgumentException($"The modifiers {invalidModifiers} are not valid for a method.", nameof(value));
        }

        public bool IsNew
        {
            get => Modifiers.Contains(New);
            set => Modifiers = Modifiers.With(New, value);
        }

        public bool IsStatic
        {
            get => Modifiers.Contains(Static);
            set => Modifiers = Modifiers.With(Static, value);
        }

        public bool IsAbstract
        {
            get => Modifiers.Contains(Abstract);
            set => Modifiers = Modifiers.With(Abstract, value);
        }

        public bool IsSealed
        {
            get => Modifiers.Contains(Sealed);
            set => Modifiers = Modifiers.With(Sealed, value);
        }

        public bool IsVirtual
        {
            get => Modifiers.Contains(Virtual);
            set => Modifiers = Modifiers.With(Virtual, value);
        }

        public bool IsOverride
        {
            get => Modifiers.Contains(Override);
            set => Modifiers = Modifiers.With(Override, value);
        }

        public bool IsPartial
        {
            get => Modifiers.Contains(Partial);
            set => Modifiers = Modifiers.With(Partial, value);
        }

        public bool IsAsync
        {
            get => Modifiers.Contains(Async);
            set => Modifiers = Modifiers.With(Async, value);
        }

        #endregion

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

        private bool explicitInterfaceSet;
        private NamedTypeReference explicitInterface;
        public NamedTypeReference ExplicitInterface
        {
            get
            {
                if (!explicitInterfaceSet)
                {
                    explicitInterface = syntax.ExplicitInterfaceSpecifier == null
                        ? null
                        : new NamedTypeReference(syntax.ExplicitInterfaceSpecifier.Name, this);
                    explicitInterfaceSet = true;
                }

                return explicitInterface;
            }
            set
            {
                Set(ref explicitInterface, value);
                explicitInterfaceSet = true;
            }
        }

        private Identifier name;
        public string Name
        {
            get => name.Text;
            set => name.Text = value;
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

        MethodDeclarationSyntax ISyntaxWrapper<MethodDeclarationSyntax>.GetWrapped(ref bool? changed)
        {
            GetAndResetChanged(ref changed);

            bool? thisChanged = false;

            var newAttributes = attributes?.GetWrapped(ref thisChanged) ?? syntax?.AttributeLists ?? default;
            var newReturnType = returnType?.GetWrapped(ref thisChanged) ?? syntax.ReturnType;
            var newExplicitInterface = explicitInterfaceSet
                ? explicitInterface?.GetWrappedName(ref thisChanged)
                : syntax.ExplicitInterfaceSpecifier?.Name;
            var newName = name.GetWrapped(ref thisChanged);
            var newTypeParameters = typeParameters?.GetWrapped(ref thisChanged) ?? syntax.TypeParameterList?.Parameters ?? default;
            var newParameters = parameters?.GetWrapped(ref thisChanged) ?? syntax.ParameterList.Parameters;
            var newConstraints = constraintClauses?.GetWrapped(ref thisChanged) ?? syntax.ConstraintClauses;
            var newBody = bodySet ? body?.GetWrapped(ref thisChanged) : syntax.Body;

            if (syntax == null || thisChanged == true || Modifiers != FromRoslyn.MemberModifiers(syntax.Modifiers) ||
                ShouldAnnotate(syntax, changed))
            {
                var explicitInterfaceSpecifier = newExplicitInterface == null
                    ? null
                    : RoslynSyntaxFactory.ExplicitInterfaceSpecifier(newExplicitInterface);

                var typeParameterList = newTypeParameters.Any() ? RoslynSyntaxFactory.TypeParameterList(newTypeParameters) : default;

                var newSyntax = RoslynSyntaxFactory.MethodDeclaration(
                    newAttributes, Modifiers.GetWrapped(), newReturnType, explicitInterfaceSpecifier, newName, typeParameterList,
                    RoslynSyntaxFactory.ParameterList(newParameters), newConstraints, newBody, null);

                syntax = Annotate(newSyntax);

                SetChanged(ref changed);
            }

            return syntax;
        }

        private protected override BaseMethodDeclarationSyntax GetWrappedBaseMethod(ref bool? changed) =>
            this.GetWrapped<MethodDeclarationSyntax>(ref changed);

        private protected override void SetSyntaxImpl(Roslyn::SyntaxNode newSyntax)
        {
            Init((MethodDeclarationSyntax)newSyntax);

            SetList(ref attributes, null);
            Set(ref returnType, null);
            Set(ref explicitInterface, null);
            explicitInterfaceSet = false;
            SetList(ref typeParameters, null);
            SetList(ref parameters, null);
            SetList(ref constraintClauses, null);
            Set(ref body, null);
        }

        private protected override SyntaxNode CloneImpl() =>
            new MethodDefinition(
                Modifiers, ReturnType, ExplicitInterface, Name, TypeParameters, Parameters, ConstraintClauses, Body?.Statements)
            {
                Attributes = Attributes
            };

        public override IEnumerable<SyntaxNode> GetChildren() =>
            Attributes.Concat<SyntaxNode>(new[] { ReturnType, ExplicitInterface }).Concat(TypeParameters).Concat(Parameters)
                .Concat(ConstraintClauses).Concat(new[] { Body });
    }
}