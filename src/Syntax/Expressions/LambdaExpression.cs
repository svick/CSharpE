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
    public sealed class LambdaExpression : Expression
    {
        private LambdaExpressionSyntax syntax;

        internal LambdaExpression(LambdaExpressionSyntax syntax, SyntaxNode parent)
            : base(syntax)
        {
            Init(syntax);
            Parent = parent;
        }

        private void Init(LambdaExpressionSyntax syntax)
        {
            this.syntax = syntax;
            IsAsync = IsSyntaxAsync();
        }

        public LambdaExpression(IEnumerable<LambdaParameter> parameters, IEnumerable<Statement> statements)
            : this(false, parameters, statements) { }

        public LambdaExpression(bool isAsync, IEnumerable<LambdaParameter> parameters, IEnumerable<Statement> statements)
        {
            if (statements == null)
                throw new ArgumentNullException(nameof(statements));

            IsAsync = isAsync;
            this.parameters = new SeparatedSyntaxList<LambdaParameter, ParameterSyntax>(parameters, this);
            this.statements = new StatementList(statements, this);
            bodySet = true;
        }

        public LambdaExpression(IEnumerable<LambdaParameter> parameters, Expression expression)
            : this(false, parameters, expression) { }

        public LambdaExpression(bool isAsync, IEnumerable<LambdaParameter> parameters, Expression expression)
        {
            IsAsync = isAsync;
            this.parameters = new SeparatedSyntaxList<LambdaParameter, ParameterSyntax>(parameters, this);
            Expression = expression;
        }

        private bool IsSyntaxAsync() => syntax.AsyncKeyword != default;

        public bool IsAsync { get; set; }

        private Roslyn::SeparatedSyntaxList<ParameterSyntax> GetSyntaxParameters()
        {
            switch (syntax)
            {
                case SimpleLambdaExpressionSyntax simple:
                    return RoslynSyntaxFactory.SingletonSeparatedList(simple.Parameter);
                case ParenthesizedLambdaExpressionSyntax parenthesized:
                    return parenthesized.ParameterList.Parameters;
            }

            throw new InvalidOperationException();
        }

        private SeparatedSyntaxList<LambdaParameter, ParameterSyntax> parameters;
        public IList<LambdaParameter> Parameters
        {
            get
            {
                if (parameters == null)
                    parameters = new SeparatedSyntaxList<LambdaParameter, ParameterSyntax>(GetSyntaxParameters(), this);

                return parameters;
            }
            set => SetList(ref parameters, new SeparatedSyntaxList<LambdaParameter, ParameterSyntax>(value, this));
        }

        private bool bodySet;

        private void SetBody()
        {
            switch (syntax.Body)
            {
                case ExpressionSyntax e:
                    expression = FromRoslyn.Expression(e, this);
                    break;
                case BlockSyntax block:
                    statements = new StatementList(block.Statements, this);
                    break;
                default:
                    throw new InvalidOperationException();
            }

            bodySet = true;
        }

        private Expression expression;
        public Expression Expression
        {
            get
            {
                if (!bodySet)
                    SetBody();

                return expression;
            }
            set
            {
                SetNotNull(ref expression, value);
                SetList(ref statements, null);
                bodySet = true;
            }
        }

        private StatementList statements;
        public IList<Statement> Statements
        {
            get
            {
                if (!bodySet)
                    SetBody();

                return statements;
            }
            set
            {
                if (value == null)
                    throw new ArgumentNullException(nameof(value));

                SetList(ref statements, new StatementList(value, this));
                Set(ref expression, null);
                bodySet = true;
            }
        }

        private protected override ExpressionSyntax GetWrappedExpression(ref bool? changed)
        {
            GetAndResetChanged(ref changed);

            bool? thisChanged = false;

            CSharpSyntaxNode GetBody()
            {
                if (expression != null)
                    return expression.GetWrapped(ref thisChanged);

                if (statements != null)
                    return RoslynSyntaxFactory.Block(statements.GetWrapped(ref thisChanged));

                return syntax.Body;
            }

            var newParameters = parameters?.GetWrapped(ref thisChanged) ?? GetSyntaxParameters();
            var newBody = GetBody();

            if (syntax == null || thisChanged == true || IsAsync != IsSyntaxAsync() || ShouldAnnotate(syntax, changed))
            {
                var asyncKeyword = IsAsync ? RoslynSyntaxFactory.Token(SyntaxKind.AsyncKeyword) : default;
                var arrowToken = RoslynSyntaxFactory.Token(SyntaxKind.EqualsGreaterThanToken);

                var firstParameter = newParameters.FirstOrDefault();

                if (newParameters.Count == 1 && firstParameter.Type == null)
                {
                    syntax = RoslynSyntaxFactory.SimpleLambdaExpression(
                        asyncKeyword, firstParameter, arrowToken, newBody);
                }
                else
                {
                    syntax = RoslynSyntaxFactory.ParenthesizedLambdaExpression(
                        asyncKeyword, RoslynSyntaxFactory.ParameterList(newParameters), arrowToken, newBody);
                }

                syntax = Annotate(syntax);

                SetChanged(ref changed);
            }

            return syntax;
        }

        private protected override void SetSyntaxImpl(Roslyn::SyntaxNode newSyntax)
        {
            SetList(ref parameters, null);
            Set(ref expression, null);
            SetList(ref statements, null);
            Init((LambdaExpressionSyntax)newSyntax);
        }

        private protected override SyntaxNode CloneImpl() =>
            Expression != null
                ? new LambdaExpression(IsAsync, Parameters, Expression)
                : new LambdaExpression(IsAsync, Parameters, Statements);

        public override IEnumerable<SyntaxNode> GetChildren() =>
            Parameters.Concat(Expression != null ? (IEnumerable<SyntaxNode>)new[] { Expression } : Statements);

        public override void ReplaceExpressions<T>(Func<T, bool> filter, Func<T, Expression> projection)
        {
            if (Expression != null)
            {
                Expression = ReplaceExpressions(Expression, filter, projection);
            }
            else
            {
                foreach (var statement in Statements)
                {
                    statement.ReplaceExpressions(filter, projection);
                }
            }
        }
    }
}