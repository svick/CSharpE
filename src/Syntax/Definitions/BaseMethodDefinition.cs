﻿using System.Collections.Generic;
using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSharpE.Syntax
{
    public abstract class BaseMethodDefinition : MemberDefinition, ISyntaxWrapper<BaseMethodDeclarationSyntax>
    {
        private protected abstract BaseMethodDeclarationSyntax BaseMethodSyntax { get; }

        private protected sealed override MemberDeclarationSyntax MemberSyntax => BaseMethodSyntax;

        public bool IsUnsafe
        {
            get => Modifiers.Contains(MemberModifiers.Unsafe);
            set => Modifiers = Modifiers.With(MemberModifiers.Unsafe, value);
        }

        public bool IsExtern
        {
            get => Modifiers.Contains(MemberModifiers.Extern);
            set => Modifiers = Modifiers.With(MemberModifiers.Extern, value);
        }

        private protected SeparatedSyntaxList<Parameter, ParameterSyntax> parameters;
        public IList<Parameter> Parameters
        {
            get
            {
                if (parameters == null)
                    parameters = new SeparatedSyntaxList<Parameter, ParameterSyntax>(
                        BaseMethodSyntax.ParameterList.Parameters, this);

                return parameters;
            }
            set => SetList(ref parameters, new SeparatedSyntaxList<Parameter, ParameterSyntax>(value, this));
        }

        // TODO: methods without body and with expression body
        private protected StatementList body;
        public IList<Statement> Body
        {
            get
            {
                if (body == null)
                    body = new StatementList(BaseMethodSyntax.Body.Statements, this);

                return body;
            }
            set => SetList(ref body, new StatementList(value, this));
        }

        BaseMethodDeclarationSyntax ISyntaxWrapper<BaseMethodDeclarationSyntax>.GetWrapped(ref bool? changed) =>
            GetWrappedBaseMethod(ref changed);

        private protected sealed override MemberDeclarationSyntax GetWrappedMember(ref bool? changed) =>
            GetWrappedBaseMethod(ref changed);

        private protected abstract BaseMethodDeclarationSyntax GetWrappedBaseMethod(ref bool? changed);
    }
}