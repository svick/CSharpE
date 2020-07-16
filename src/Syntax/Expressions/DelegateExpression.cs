using System;
using System.Collections.Generic;
using System.Linq;
using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RoslynSyntaxFactory = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Roslyn = Microsoft.CodeAnalysis;

namespace CSharpE.Syntax
{
    public sealed class DelegateExpression : Expression
    {
        private AnonymousMethodExpressionSyntax syntax;

        internal DelegateExpression(AnonymousMethodExpressionSyntax syntax, SyntaxNode parent)
            : base(syntax)
        {
            Init(syntax);
            Parent = parent;
        }

        private void Init(AnonymousMethodExpressionSyntax syntax)
        {
            this.syntax = syntax;
            IsAsync = IsSyntaxAsync();
        }

        public DelegateExpression(IEnumerable<LambdaParameter> parameters, IEnumerable<Statement> statements)
            : this(false, parameters, statements) { }

        public DelegateExpression(bool isAsync, IEnumerable<LambdaParameter> parameters, IEnumerable<Statement> statements)
        {
            if (statements == null)
                throw new ArgumentNullException(nameof(statements));

            IsAsync = isAsync;
            this.parameters = parameters == null
                ? null
                : new SeparatedSyntaxList<LambdaParameter, ParameterSyntax>(parameters, this);
            parametersSet = true;
            this.statements = new StatementList(statements, this);
        }

        private bool IsSyntaxAsync() => syntax.AsyncKeyword != default;

        public bool IsAsync { get; set; }

        private bool parametersSet;
        private SeparatedSyntaxList<LambdaParameter, ParameterSyntax> parameters;
        public IList<LambdaParameter> Parameters
        {
            get
            {
                if (!parametersSet)
                {
                    parameters =
                        syntax.ParameterList == null
                            ? null
                            : new SeparatedSyntaxList<LambdaParameter, ParameterSyntax>(
                                syntax.ParameterList.Parameters, this);
                    parametersSet = true;
                }

                return parameters;
            }
            set
            {
                SetList(
                    ref parameters,
                    value == null ? null : new SeparatedSyntaxList<LambdaParameter, ParameterSyntax>(value, this));
                parametersSet = true;
            }
        }

        private StatementList statements;
        public IList<Statement> Statements
        {
            get
            {
                if (statements == null)
                    statements = new StatementList(syntax.Block.Statements, this);

                return statements;
            }
            set => SetList(ref statements, new StatementList(value, this));
        }

        private protected override ExpressionSyntax GetWrappedExpression(ref bool? changed)
        {
            GetAndResetChanged(ref changed, out var thisChanged);

            var newParameters =
                parametersSet ? parameters?.GetWrapped(ref thisChanged) : syntax.ParameterList?.Parameters;
            var newBody = statements == null ? syntax.Body : RoslynSyntaxFactory.Block(statements.GetWrapped(ref thisChanged));

            if (syntax == null || thisChanged == true || IsAsync != IsSyntaxAsync() || ShouldAnnotate(syntax, changed))
            {
                var asyncKeyword = IsAsync ? RoslynSyntaxFactory.Token(SyntaxKind.AsyncKeyword) : default;
                var parameterList = newParameters == null ? null : RoslynSyntaxFactory.ParameterList(newParameters.Value);

                syntax = RoslynSyntaxFactory.AnonymousMethodExpression(
                    asyncKeyword, RoslynSyntaxFactory.Token(SyntaxKind.DelegateKeyword), parameterList, newBody);

                syntax = Annotate(syntax);

                SetChanged(ref changed);
            }

            return syntax;
        }

        private protected override void SetSyntaxImpl(Roslyn::SyntaxNode newSyntax)
        {
            Init((AnonymousMethodExpressionSyntax)newSyntax);
            SetList(ref parameters, null);
            SetList(ref statements, null);
        }

        private protected override SyntaxNode CloneImpl() => new DelegateExpression(IsAsync, Parameters, Statements);

        public override IEnumerable<SyntaxNode> GetChildren() =>
            (Parameters ?? Enumerable.Empty<SyntaxNode>()).Concat(Statements);

        public override void ReplaceExpressions<T>(Func<T, bool> filter, Func<T, Expression> projection)
        {
            foreach (var statement in Statements)
            {
                statement.ReplaceExpressions(filter, projection);
            }
        }
    }
}