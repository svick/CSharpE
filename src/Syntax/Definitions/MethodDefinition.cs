using System;
using System.Collections.Generic;
using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static CSharpE.Syntax.MemberModifiers;
using CSharpSyntaxFactory = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Roslyn = Microsoft.CodeAnalysis;

namespace CSharpE.Syntax
{
    public sealed class MethodDefinition : MemberDefinition, ISyntaxWrapper<MethodDeclarationSyntax>
    {
        private MethodDeclarationSyntax syntax;

        internal MethodDefinition(MethodDeclarationSyntax syntax, TypeDefinition parent)
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

        protected override SyntaxList<AttributeListSyntax> GetSyntaxAttributes() => syntax?.AttributeLists ?? default;

        #region Modifiers

        private const MemberModifiers ValidMethodModifiers =
            AccessModifiersMask | New | Static | Unsafe | Abstract | Sealed | Virtual | Override | Extern | Partial |
            Async;

        protected override void ValidateModifiers(MemberModifiers value)
        {
            var invalidModifiers = value & ~ValidMethodModifiers;
            if (invalidModifiers != 0)
                throw new ArgumentException(nameof(value), $"The modifiers {invalidModifiers} are not valid for a method.");
        }

        public MemberModifiers Accessibility
        {
            get => Modifiers.Accessibility();
            set => Modifiers = Modifiers.WithAccessibilityModifier(value);
        }

        public bool IsPublic => Accessibility == Public;
        public bool IsProtected => Accessibility == Protected;
        public bool IsInternal => Accessibility == Internal;
        public bool IsPrivate => Accessibility == Private;
        public bool IsProtectedInternal => Accessibility == ProtectedInternal;

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

        public bool IsUnsafe
        {
            get => Modifiers.Contains(Unsafe);
            set => Modifiers = Modifiers.With(Unsafe, value);
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

        public bool IsExtern
        {
            get => Modifiers.Contains(Extern);
            set => Modifiers = Modifiers.With(Extern, value);
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

        private Identifier name;
        public string Name
        {
            get => name.Text;
            set => name.Text = value;
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

        // TODO: methods without body and with expression body
        private StatementList body;
        public IList<Statement> Body
        {
            get
            {
                if (body == null)
                    body = new StatementList(syntax.Body.Statements, this);

                return body;
            }
            set => SetList(ref body, new StatementList(value, this));
        }

        public TypeDefinition ParentType { get; private set; }

        internal MethodDeclarationSyntax GetWrapped(ref bool? changed)
        {
            GetAndResetChanged(ref changed);

            bool? thisChanged = false;

            var newModifiers = Modifiers;
            var newReturnType = returnType?.GetWrapped(ref thisChanged) ?? syntax.ReturnType;
            var newName = name.GetWrapped(ref thisChanged);
            var newParameters = parameters?.GetWrapped(ref thisChanged) ?? syntax.ParameterList.Parameters;
            var newBody = body?.GetWrapped(ref thisChanged) ?? syntax.Body.Statements;

            if (syntax == null || AttributesChanged() || newModifiers != FromRoslyn.MemberModifiers(syntax.Modifiers) ||
                thisChanged == true || !IsAnnotated(syntax))
            {
                var newSyntax = CSharpSyntaxFactory.MethodDeclaration(
                    GetNewAttributes(), newModifiers.GetWrapped(), newReturnType, null, newName, null,
                    CSharpSyntaxFactory.ParameterList(newParameters), default, CSharpSyntaxFactory.Block(newBody),
                    null);

                syntax = Annotate(newSyntax);

                SetChanged(ref changed);
            }

            return syntax;
        }

        protected override MemberDeclarationSyntax GetWrappedImpl(ref bool? changed) => GetWrapped(ref changed);

        MethodDeclarationSyntax ISyntaxWrapper<MethodDeclarationSyntax>.GetWrapped(ref bool? changed) =>
            GetWrapped(ref changed);

        protected override void SetSyntaxImpl(Roslyn::SyntaxNode newSyntax)
        {
            Init((MethodDeclarationSyntax)newSyntax);
            ResetAttributes();

            Set(ref returnType, null);
            parameters = null;
            body = null;
        }

        internal override SyntaxNode Clone()
        {
            throw new NotImplementedException();
        }

        internal override SyntaxNode Parent
        {
            get => ParentType;
            set
            {
                if (value is TypeDefinition parentType)
                    ParentType = parentType;
                else
                    throw new ArgumentException(nameof(value));
            }
        }
    }
}