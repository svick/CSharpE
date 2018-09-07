﻿using System;
using System.Collections.Generic;
using System.Linq;
using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static CSharpE.Syntax.MemberModifiers;
using CSharpSyntaxFactory = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Roslyn = Microsoft.CodeAnalysis;

namespace CSharpE.Syntax
{
    public sealed class MethodDefinition : BaseMethodDefinition, ISyntaxWrapper<MethodDeclarationSyntax>
    {
        private MethodDeclarationSyntax syntax;

        private protected override BaseMethodDeclarationSyntax BaseMethodSyntax => syntax;

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

        public MethodDefinition(
            MemberModifiers modifiers, TypeReference returnType, string name, IEnumerable<Parameter> parameters,
            IEnumerable<Statement> body)
        {
            Modifiers = modifiers;
            ReturnType = returnType;
            Name = name;
            Parameters = parameters.ToList();
            Body = body.ToList();
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

        private Identifier name;
        public string Name
        {
            get => name.Text;
            set => name.Text = value;
        }


        MethodDeclarationSyntax ISyntaxWrapper<MethodDeclarationSyntax>.GetWrapped(ref bool? changed)
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

        private protected override BaseMethodDeclarationSyntax GetWrappedBaseMethod(ref bool? changed) =>
            this.GetWrapped<MethodDeclarationSyntax>(ref changed);

        private protected override void SetSyntaxImpl(Roslyn::SyntaxNode newSyntax)
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
    }
}