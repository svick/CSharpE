using System;
using System.Collections.Generic;
using System.Linq;
using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RoslynSyntaxFactory = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Roslyn = Microsoft.CodeAnalysis;

namespace CSharpE.Syntax
{
    public sealed class LinqExpression : Expression
    {
        private QueryExpressionSyntax syntax;

        internal LinqExpression(QueryExpressionSyntax syntax, SyntaxNode parent)
            : base(syntax)
        {
            this.syntax = syntax;
            Parent = parent;
        }

        public LinqExpression(params LinqClause[] clauses)
            : this(clauses.AsEnumerable()) { }

        public LinqExpression(IEnumerable<LinqClause> clauses) =>
            this.clauses = new LinqClauseList(clauses, this);

        private List<Roslyn::SyntaxNode> GetSyntaxClauses()
        {
            var syntaxClauses = new List<Roslyn::SyntaxNode>();

            syntaxClauses.Add(syntax.FromClause);

            AddBody(syntax.Body);

            void AddBody(QueryBodySyntax body)
            {
                syntaxClauses.AddRange(body.Clauses);

                if (body.Continuation == null)
                {
                    syntaxClauses.Add(body.SelectOrGroup);
                }
                else
                {
                    syntaxClauses.Add(WithIntoAnnotation(body.SelectOrGroup, body.Continuation.Identifier.ValueText));

                    AddBody(body.Continuation.Body);
                }
            }

            return syntaxClauses;
        }

        private LinqClauseList clauses;
        public IList<LinqClause> Clauses
        {
            get
            {
                if (clauses == null)
                    clauses = new LinqClauseList(GetSyntaxClauses(), this);

                return clauses;
            }
            set => SetList(ref clauses, new LinqClauseList(value, this));
        }

        private const string IntoAnnotationKind = "CSharpE.IntoAnnotation";

        internal static TClause WithIntoAnnotation<TClause>(TClause clause, string into)
            where TClause : SelectOrGroupClauseSyntax =>
            into == null ? clause : clause.WithAdditionalAnnotations(new SyntaxAnnotation(IntoAnnotationKind, into));

        internal static string GetSyntaxInto(SelectOrGroupClauseSyntax selectOrGroup) =>
            selectOrGroup.GetAnnotations(IntoAnnotationKind).SingleOrDefault()?.Data;

        private protected override ExpressionSyntax GetWrappedExpression(ref bool? changed)
        {
            GetAndResetChanged(ref changed);

            bool? thisChanged = false;

            var newClauses = clauses?.GetWrapped(ref thisChanged) ?? GetSyntaxClauses();

            if (syntax == null || thisChanged == true || ShouldAnnotate(syntax, changed))
            {
                var clausesStack = new Stack<QueryClauseSyntax>();
                SelectOrGroupClauseSyntax selectOrGroup = null;
                QueryContinuationSyntax continuation = null;

                QueryBodySyntax CreateBody()
                {
                    var body = RoslynSyntaxFactory.QueryBody(
                        RoslynSyntaxFactory.List(clausesStack), selectOrGroup, continuation);

                    clausesStack.Clear();
                    selectOrGroup = null;
                    continuation = null;

                    return body;
                }

                newClauses.Reverse();

                foreach (var clause in newClauses)
                {
                    if (clause is SelectOrGroupClauseSyntax sog)
                    {
                        string into = GetSyntaxInto(sog);

                        if (selectOrGroup == null)
                        {
                            if (into != null)
                                throw new InvalidOperationException(
                                    "select or group by with into cannot be the last clause in a query.");

                            selectOrGroup = sog;
                        }
                        else
                        {
                            if (into == null)
                                throw new InvalidOperationException(
                                    "select or group by without into cannot be in the middle of a query.");

                            continuation = RoslynSyntaxFactory.QueryContinuation(into, CreateBody());

                            selectOrGroup = sog.WithoutAnnotations(IntoAnnotationKind);
                        }
                    }
                    else
                    {
                        clausesStack.Push((QueryClauseSyntax)clause);
                    }
                }

                syntax = RoslynSyntaxFactory.QueryExpression((FromClauseSyntax)clausesStack.Pop(), CreateBody());

                syntax = Annotate(syntax);

                SetChanged(ref changed);
            }

            return syntax;
        }

        private protected override void SetSyntaxImpl(Roslyn::SyntaxNode newSyntax)
        {
            syntax = (QueryExpressionSyntax)newSyntax;

            SetList(ref clauses, null);
        }

        private protected override SyntaxNode CloneImpl() => new LinqExpression(Clauses);

        public override void ReplaceExpressions<T>(Func<T, bool> filter, Func<T, Expression> projection)
        {
            foreach (var clause in Clauses)
            {
                clause.ReplaceExpressions(filter, projection);
            }
        }
    }
}