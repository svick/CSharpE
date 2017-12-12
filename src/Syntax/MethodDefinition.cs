using System;
using System.Collections.Generic;
using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static CSharpE.Syntax.MemberModifiers;
using CSharpSyntaxFactory = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace CSharpE.Syntax
{
    public class MethodDefinition : MemberDefinition
    {
        private MethodDeclarationSyntax syntax;

        internal MethodDefinition(MethodDeclarationSyntax syntax, TypeDefinition containingType)
        {
            this.syntax = syntax;
            ContainingType = containingType;

            Modifiers = FromRoslyn.MemberModifiers(syntax.Modifiers);
            name = new Identifier(syntax.Identifier);
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

        public TypeReference ReturnType { get; set; }

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
                    parameters = new SeparatedSyntaxList<Parameter, ParameterSyntax>(syntax.ParameterList.Parameters);

                return parameters;
            }
            set => parameters = new SeparatedSyntaxList<Parameter, ParameterSyntax>(value);
        }

        // TODO: methods without body and with expression body
        private SyntaxList<Statement, StatementSyntax> body;
        public IList<Statement> Body
        {
            get
            {
                if (body == null)
                    body = new SyntaxList<Statement, StatementSyntax>(syntax.Body.Statements, FromRoslyn.Statement);

                return body;
            }
            set => body = new SyntaxList<Statement, StatementSyntax>(value);
        }


        internal new MethodDeclarationSyntax GetWrapped(WrapperContext context)
        {
            var newModifiers = Modifiers;
            var newReturnType = ReturnType?.GetWrapped(context) ?? syntax.ReturnType;
            var newName = name.GetWrapped(context);
            var newParameters = parameters?.GetWrapped(context) ?? syntax.ParameterList.Parameters;
            var newBody = body?.GetWrapped(context) ?? syntax.Body.Statements;

            if (syntax == null || AttributesChanged() || newModifiers != FromRoslyn.MemberModifiers(syntax.Modifiers) ||
                newReturnType != syntax.ReturnType || newName != syntax.Identifier ||
                newParameters != syntax.ParameterList.Parameters || newBody != syntax.Body.Statements)
            {
                syntax = CSharpSyntaxFactory.MethodDeclaration(
                    GetNewAttributes(context), newModifiers.GetWrapped(), newReturnType, null, newName, null,
                    CSharpSyntaxFactory.ParameterList(newParameters), default, CSharpSyntaxFactory.Block(newBody),
                    null);
            }

            return syntax;
        }

        protected override MemberDeclarationSyntax GetWrappedImpl(WrapperContext context) => GetWrapped(context);
    }
}