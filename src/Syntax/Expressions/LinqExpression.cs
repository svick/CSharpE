using System;
using System.Collections.Generic;
using System.Linq;
using CSharpE.Syntax.Internals;
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

                syntaxClauses.Add(body.SelectOrGroup);

                if (body.Continuation != null)
                {
                    syntaxClauses.Add(RoslynSyntaxFactory.IdentifierName(body.Continuation.Identifier));

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
                    if (selectOrGroup == null)
                    {
                        selectOrGroup = (SelectOrGroupClauseSyntax)clause;
                    }
                    else if (clause is IdentifierNameSyntax identifier)
                    {
                        continuation = RoslynSyntaxFactory.QueryContinuation(identifier.Identifier, CreateBody());
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

        public override IEnumerable<SyntaxNode> GetChildren() => Clauses;

        public override void ReplaceExpressions<T>(Func<T, bool> filter, Func<T, Expression> projection)
        {
            foreach (var clause in Clauses)
            {
                clause.ReplaceExpressions(filter, projection);
            }
        }
    }
}