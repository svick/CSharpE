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

        public LambdaExpression(IEnumerable<LambdaParameter> parameters, BlockStatement statementBody)
            : this(false, parameters, statementBody) { }

        public LambdaExpression(bool isAsync, IEnumerable<LambdaParameter> parameters, BlockStatement statementBody)
        {
            IsAsync = isAsync;
            Parameters = parameters?.ToList();
            StatementBody = statementBody;
        }

        public LambdaExpression(IEnumerable<LambdaParameter> parameters, Expression expressionBody)
            : this(false, parameters, expressionBody) { }

        public LambdaExpression(bool isAsync, IEnumerable<LambdaParameter> parameters, Expression expressionBody)
        {
            IsAsync = isAsync;
            Parameters = parameters?.ToList();
            ExpressionBody = expressionBody;
        }

        private bool IsSyntaxAsync() => syntax.AsyncKeyword != default;

        public bool IsAsync { get; set; }

        private Roslyn::SeparatedSyntaxList<ParameterSyntax> GetSyntaxParameters() =>
            syntax switch
            {
                SimpleLambdaExpressionSyntax simple => RoslynSyntaxFactory.SingletonSeparatedList(simple.Parameter),
                ParenthesizedLambdaExpressionSyntax parenthesized => parenthesized.ParameterList.Parameters,
                _ => throw new InvalidOperationException(),
            };

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
                    expressionBody = FromRoslyn.Expression(e, this);
                    break;
                case BlockSyntax block:
                    statementBody = new BlockStatement(block, this);
                    break;
                default:
                    throw new InvalidOperationException();
            }

            bodySet = true;
        }

        private Expression expressionBody;
        public Expression ExpressionBody
        {
            get
            {
                if (!bodySet)
                    SetBody();

                return expressionBody;
            }
            set
            {
                Set(ref expressionBody, value);
                bodySet = true;

                if (value != null)
                    StatementBody = null;
            }
        }

        private BlockStatement statementBody;
        public BlockStatement StatementBody
        {
            get
            {
                if (!bodySet)
                    SetBody();

                return statementBody;
            }
            set
            {
                Set(ref statementBody, value);
                bodySet = true;

                if (value != null)
                    ExpressionBody = null;
            }
        }

        private protected override ExpressionSyntax GetWrappedExpression(ref bool? changed)
        {
            GetAndResetChanged(ref changed, out var thisChanged);

            CSharpSyntaxNode GetBody()
            {
                if (expressionBody != null)
                    return expressionBody.GetWrapped(ref thisChanged);

                if (statementBody != null)
                    return statementBody.GetWrapped(ref thisChanged);

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
            Set(ref expressionBody, null);
            Set(ref statementBody, null);
            bodySet = false;

            Init((LambdaExpressionSyntax)newSyntax);
        }

        private protected override SyntaxNode CloneImpl() =>
            ExpressionBody != null
                ? new LambdaExpression(IsAsync, Parameters, ExpressionBody)
                : new LambdaExpression(IsAsync, Parameters, StatementBody);

        public override void ReplaceExpressions<T>(Func<T, bool> filter, Func<T, Expression> projection)
        {
            if (ExpressionBody != null)
            {
                ExpressionBody = ReplaceExpressions(ExpressionBody, filter, projection);
            }
            else
            {
                StatementBody.ReplaceExpressions(filter, projection);
            }
        }
    }
}