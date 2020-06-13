using System;
using System.Collections.Generic;
using System.Linq;
using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RoslynSyntaxFactory = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Roslyn = Microsoft.CodeAnalysis;
using static CSharpE.Syntax.MemberModifiers;

namespace CSharpE.Syntax
{
    public sealed class LocalFunctionStatement : Statement
    {
        private LocalFunctionStatementSyntax syntax;

        internal LocalFunctionStatement(LocalFunctionStatementSyntax syntax, SyntaxNode parent)
            : base(syntax)
        {
            Init(syntax);
            Parent = parent;
        }

        private void Init(LocalFunctionStatementSyntax syntax)
        {
            this.syntax = syntax;

            modifiers = FromRoslyn.MemberModifiers(syntax.Modifiers);

            name = new Identifier(syntax.Identifier);
        }

        public LocalFunctionStatement(
            TypeReference returnType, string name, IEnumerable<Parameter> parameters, BlockStatement statementBody)
            : this(default, returnType, name, parameters, statementBody) { }

        public LocalFunctionStatement(
            MemberModifiers modifiers, TypeReference returnType, string name, IEnumerable<Parameter> parameters, BlockStatement statementBody)
            : this(modifiers, returnType, name, null, parameters, null, statementBody) { }

        public LocalFunctionStatement(
            MemberModifiers modifiers, TypeReference returnType, string name, IEnumerable<TypeParameter> typeParameters,
            IEnumerable<Parameter> parameters, IEnumerable<TypeParameterConstraintClause> constraintClauses, BlockStatement statementBody)
            : this(modifiers, returnType, name, typeParameters, parameters, constraintClauses, statementBody, null) { }

        public LocalFunctionStatement(
            TypeReference returnType, string name, IEnumerable<Parameter> parameters, Expression expressionBody)
            : this(default, returnType, name, parameters, expressionBody) { }

        public LocalFunctionStatement(
            MemberModifiers modifiers, TypeReference returnType, string name, IEnumerable<Parameter> parameters, Expression expressionBody)
            : this(modifiers, returnType, name, null, parameters, null, expressionBody) { }

        public LocalFunctionStatement(
            MemberModifiers modifiers, TypeReference returnType, string name, IEnumerable<TypeParameter> typeParameters,
            IEnumerable<Parameter> parameters, IEnumerable<TypeParameterConstraintClause> constraintClauses, Expression expressionBody)
            : this(modifiers, returnType, name, typeParameters, parameters, constraintClauses, null, expressionBody) { }

        private LocalFunctionStatement(
            MemberModifiers modifiers, TypeReference returnType, string name, IEnumerable<TypeParameter> typeParameters,
            IEnumerable<Parameter> parameters, IEnumerable<TypeParameterConstraintClause> constraintClauses,
            BlockStatement statementBody, Expression expressionBody)
        {
            Modifiers = modifiers;
            ReturnType = returnType;
            Name = name;
            TypeParameters = typeParameters?.ToList();
            Parameters = parameters?.ToList();
            ConstraintClauses = constraintClauses?.ToList();
            StatementBody = statementBody;
            ExpressionBody = expressionBody;
        }

        private MemberModifiers modifiers;
        public MemberModifiers Modifiers
        {
            get => modifiers;
            set
            {
                ValidateModifiers(value);
                modifiers = value;
            }
        }

        private const MemberModifiers ValidLocalFunctionModifiers = Async | Unsafe | Static;

        private void ValidateModifiers(MemberModifiers value)
        {
            var invalidModifiers = value & ~ValidLocalFunctionModifiers;
            if (invalidModifiers != 0)
                throw new ArgumentException($"The modifiers {invalidModifiers} are not valid for a local function.", nameof(value));
        }


        public bool IsAsync
        {
            get => modifiers.Contains(Async);
            set => modifiers = modifiers.With(Async, value);
        }

        public bool IsUnsafe
        {
            get => modifiers.Contains(Unsafe);
            set => modifiers = modifiers.With(Unsafe, value);
        }

        public bool IsStatic
        {
            get => modifiers.Contains(Static);
            set => modifiers = modifiers.With(Static, value);
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

        private bool statementBodySet;
        private BlockStatement statementBody;
        public BlockStatement StatementBody
        {
            get
            {
                if (!statementBodySet)
                {
                    statementBody = syntax.Body == null ? null : new BlockStatement(syntax.Body, this);
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

        private bool expressionBodySet;
        private Expression expressionBody;
        public Expression ExpressionBody
        {
            get
            {
                if (!expressionBodySet)
                {
                    expressionBody = syntax.ExpressionBody == null
                        ? null
                        : FromRoslyn.Expression(syntax.ExpressionBody.Expression, this);
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

        private protected override StatementSyntax GetWrappedStatement(ref bool? changed)
        {
            GetAndResetChanged(ref changed, out var thisChanged);

            var newReturnType = returnType?.GetWrapped(ref thisChanged) ?? syntax.ReturnType;
            var newName = name.GetWrapped(ref thisChanged);
            var newTypeParameters = typeParameters?.GetWrapped(ref thisChanged) ?? syntax.TypeParameterList?.Parameters ?? default;
            var newParameters = parameters?.GetWrapped(ref thisChanged) ?? syntax.ParameterList.Parameters;
            var newConstraints = constraintClauses?.GetWrapped(ref thisChanged) ?? syntax.ConstraintClauses;
            var newStatementBody = statementBodySet ? statementBody?.GetWrapped(ref thisChanged) : syntax.Body;
            var newExpressionBody = expressionBodySet ? expressionBody?.GetWrapped(ref thisChanged) : syntax.ExpressionBody?.Expression;

            if (syntax == null || thisChanged == true || modifiers != FromRoslyn.MemberModifiers(syntax.Modifiers) ||
                ShouldAnnotate(syntax, changed))
            {
                var typeParameterList = newTypeParameters.Any() ? RoslynSyntaxFactory.TypeParameterList(newTypeParameters) : default;

                var arrowClause = newExpressionBody == null ? null : RoslynSyntaxFactory.ArrowExpressionClause(newExpressionBody);
                var semicolonToken = newStatementBody == null ? RoslynSyntaxFactory.Token(SyntaxKind.SemicolonToken) : default;

                syntax = RoslynSyntaxFactory.LocalFunctionStatement(
                    modifiers.GetWrapped(), newReturnType, newName, typeParameterList,
                    RoslynSyntaxFactory.ParameterList(newParameters), newConstraints, newStatementBody, arrowClause, semicolonToken);

                syntax = Annotate(syntax);

                SetChanged(ref changed);
            }

            return syntax;
        }

        private protected override void SetSyntaxImpl(Roslyn::SyntaxNode newSyntax)
        {
            Init((LocalFunctionStatementSyntax)newSyntax);
            Set(ref returnType, null);
            SetList(ref typeParameters, null);
            SetList(ref parameters, null);
            SetList(ref constraintClauses, null);
            Set(ref statementBody, null);
        }

        private protected override SyntaxNode CloneImpl() => new LocalFunctionStatement(
                Modifiers, ReturnType, Name, TypeParameters, Parameters, ConstraintClauses, StatementBody, ExpressionBody);

        public override void ReplaceExpressions<T>(Func<T, bool> filter, Func<T, Expression> projection) =>
            StatementBody.ReplaceExpressions(filter, projection);
    }
}