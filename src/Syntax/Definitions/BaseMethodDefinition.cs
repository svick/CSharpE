using System;
using System.Collections.Generic;
using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSharpE.Syntax
{
    public abstract class BaseMethodDefinition : MemberDefinition, ISyntaxWrapper<BaseMethodDeclarationSyntax>
    {
        internal BaseMethodDefinition() { }

        internal BaseMethodDefinition(BaseMethodDeclarationSyntax syntax)
            : base(syntax) { }

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

        private protected bool statementBodySet;
        private protected BlockStatement statementBody;
        public BlockStatement StatementBody
        {
            get
            {
                if (!statementBodySet)
                {
                    statementBody = BaseMethodSyntax.Body == null ? null : new BlockStatement(BaseMethodSyntax.Body, this); 
                    statementBodySet = true;
                }

                return statementBody;
            }
            set
            {
                Set(ref statementBody, value);
                statementBodySet = true;

                if (value != null)
                    ExpressionBody = null;
            }
        }

        private protected bool expressionBodySet;
        private protected Expression expressionBody;
        public Expression ExpressionBody
        {
            get
            {
                if (!expressionBodySet)
                {
                    expressionBody = BaseMethodSyntax.ExpressionBody == null
                        ? null
                        : FromRoslyn.Expression(BaseMethodSyntax.ExpressionBody.Expression, this);
                    expressionBodySet = true;
                }

                return expressionBody;
            }
            set
            {
                Set(ref expressionBody, value);
                expressionBodySet = true;

                if (value != null)
                    StatementBody = null;
            }
        }

        BaseMethodDeclarationSyntax ISyntaxWrapper<BaseMethodDeclarationSyntax>.GetWrapped(ref bool? changed) =>
            GetWrappedBaseMethod(ref changed);

        private protected sealed override MemberDeclarationSyntax GetWrappedMember(ref bool? changed) =>
            GetWrappedBaseMethod(ref changed);

        private protected abstract BaseMethodDeclarationSyntax GetWrappedBaseMethod(ref bool? changed);

        protected override void ReplaceExpressionsImpl<T>(Func<T, bool> filter, Func<T, Expression> projection)
        {
            StatementBody?.ReplaceExpressions(filter, projection);
            ExpressionBody = Expression.ReplaceExpressions(ExpressionBody, filter, projection);
        }
    }
}