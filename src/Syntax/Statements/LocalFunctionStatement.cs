using System;
using System.Collections.Generic;
using System.Linq;
using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RoslynSyntaxFactory = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Roslyn = Microsoft.CodeAnalysis;
using static CSharpE.Syntax.MemberModifiers;

namespace CSharpE.Syntax
{
    // TODO: expression body
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
            TypeReference returnType, string name, IEnumerable<Parameter> parameters, params Statement[] body)
            : this(returnType, name, parameters, body.AsEnumerable()) { }

        public LocalFunctionStatement(
            TypeReference returnType, string name, IEnumerable<Parameter> parameters, IEnumerable<Statement> body)
            : this(false, false, returnType, name, parameters, body) { }

        public LocalFunctionStatement(
            bool isAsync, bool isUnsafe, TypeReference returnType, string name, IEnumerable<Parameter> parameters,
            IEnumerable<Statement> body)
        { }

        public LocalFunctionStatement(
            bool isAsync, bool isUnsafe, TypeReference returnType, string name, IEnumerable<TypeParameter> typeParameters,
            IEnumerable<Parameter> parameters, IEnumerable<TypeParameterConstraintClause> constraintClauses, IEnumerable<Statement> body)
        {
            IsAsync = isAsync;
            IsUnsafe = isUnsafe;
            ReturnType = returnType;
            this.name = new Identifier(name);
            TypeParameters = typeParameters?.ToList();
            Parameters = parameters?.ToList();
            ConstraintClauses = constraintClauses?.ToList();
            this.body = new BlockStatement(body);
        }

        private MemberModifiers modifiers;

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

        private BlockStatement body;
        public BlockStatement Body
        {
            get
            {
                if (body == null)
                    body = new BlockStatement(syntax.Body, this);

                return body;
            }
            set => SetNotNull(ref body, value);
        }

        private protected override StatementSyntax GetWrappedStatement(ref bool? changed)
        {
            GetAndResetChanged(ref changed);

            bool? thisChanged = false;

            var newReturnType = returnType?.GetWrapped(ref thisChanged) ?? syntax.ReturnType;
            var newName = name.GetWrapped(ref thisChanged);
            var newTypeParameters = typeParameters?.GetWrapped(ref thisChanged) ?? syntax.TypeParameterList?.Parameters ?? default;
            var newParameters = parameters?.GetWrapped(ref thisChanged) ?? syntax.ParameterList.Parameters;
            var newConstraints = constraintClauses?.GetWrapped(ref thisChanged) ?? syntax.ConstraintClauses;
            var newBody = body?.GetWrapped(ref thisChanged) ?? syntax.Body;

            if (syntax == null || thisChanged == true || modifiers != FromRoslyn.MemberModifiers(syntax.Modifiers) ||
                ShouldAnnotate(syntax, changed))
            {
                var typeParameterList = newTypeParameters.Any() ? RoslynSyntaxFactory.TypeParameterList(newTypeParameters) : default;

                syntax = RoslynSyntaxFactory.LocalFunctionStatement(
                    modifiers.GetWrapped(), newReturnType, newName, typeParameterList,
                    RoslynSyntaxFactory.ParameterList(newParameters), newConstraints, newBody, null);

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
            Set(ref body, null);
        }

        private protected override SyntaxNode CloneImpl() =>
            new LocalFunctionStatement(IsAsync, IsUnsafe, ReturnType, Name, TypeParameters, Parameters, ConstraintClauses, Body.Statements);

        public override IEnumerable<SyntaxNode> GetChildren() =>
            new SyntaxNode[] { ReturnType }.Concat(TypeParameters).Concat(Parameters).Concat(ConstraintClauses).Concat(new[] { Body });

        public override void ReplaceExpressions<T>(Func<T, bool> filter, Func<T, Expression> projection) =>
            Body.ReplaceExpressions(filter, projection);
    }
}