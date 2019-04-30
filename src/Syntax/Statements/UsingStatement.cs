using System;
using System.Collections.Generic;
using System.Linq;
using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RoslynSyntaxFactory = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Roslyn = Microsoft.CodeAnalysis;

namespace CSharpE.Syntax
{
    public sealed class UsingStatement : Statement
    {
        private UsingStatementSyntax syntax;

        internal UsingStatement(UsingStatementSyntax syntax, SyntaxNode parent) : base(syntax)
        {
            this.syntax = syntax;
            Parent = parent;
        }

        public UsingStatement(VariableDeclarationStatement variableDeclaration, params Statement[] statements)
            : this(variableDeclaration, statements.AsEnumerable()) { }

        public UsingStatement(VariableDeclarationStatement variableDeclaration, IEnumerable<Statement> statements)
        {
            VariableDeclaration = variableDeclaration;
            this.statements = new StatementList(statements, this);
        }

        public UsingStatement(Expression expression, params Statement[] statements)
            : this(expression, statements.AsEnumerable()) { }

        public UsingStatement(Expression expression, IEnumerable<Statement> statements)
        {
            Expression = expression;
            this.statements = new StatementList(statements, this);
        }

        private bool variableDeclarationSet;
        private VariableDeclarationStatement variableDeclaration;
        public VariableDeclarationStatement VariableDeclaration
        {
            get
            {
                if (!variableDeclarationSet)
                {
                    variableDeclaration = syntax.Declaration == null
                        ? null
                        : new VariableDeclarationStatement(
                            RoslynSyntaxFactory.LocalDeclarationStatement(syntax.Declaration), this);

                    variableDeclarationSet = true;
                }

                return variableDeclaration;
            }
            set
            {
                Set(ref variableDeclaration, value);
                variableDeclarationSet = true;
                Set(ref expression, null);
            }
        }

        private bool expressionSet;
        private Expression expression;
        public Expression Expression
        {
            get
            {
                if (!expressionSet)
                {
                    expression = FromRoslyn.Expression(syntax.Expression, this);
                    expressionSet = true;
                }

                return expression;
            }
            set
            {
                Set(ref expression, value);
                expressionSet = true;
                Set(ref variableDeclaration, null);
            }
        }

        private StatementList statements;
        public IList<Statement> Statements
        {
            get
            {
                if (statements == null)
                    statements = new StatementList(GetStatementList(syntax.Statement), this);

                return statements;
            }
            set => SetList(ref statements, new StatementList(value, this));
        }

        private protected override StatementSyntax GetWrappedStatement(ref bool? changed)
        {
            GetAndResetChanged(ref changed);

            bool? thisChanged = false;

            var newVariable = variableDeclaration?.GetWrapped(ref thisChanged).Declaration ?? syntax.Declaration;
            var newExpression = expression?.GetWrapped(ref thisChanged) ?? syntax.Expression;
            var newStatements = statements?.GetWrapped(ref thisChanged) ?? GetStatementList(syntax.Statement);

            if (syntax == null || thisChanged == true)
            {
                syntax = RoslynSyntaxFactory.UsingStatement(
                    newVariable, newExpression, RoslynSyntaxFactory.Block(newStatements));

                SetChanged(ref changed);
            }

            return syntax;
        }

        private protected override void SetSyntaxImpl(Roslyn::SyntaxNode newSyntax)
        {
            syntax = (UsingStatementSyntax)newSyntax;

            Set(ref variableDeclaration, null);
            variableDeclarationSet = false;
            Set(ref expression, null);
            expressionSet = false;
            SetList(ref statements, null);
        }

        internal override SyntaxNode Clone() => VariableDeclaration != null
            ? new UsingStatement(VariableDeclaration, Statements)
            : new UsingStatement(Expression, Statements);

        internal override SyntaxNode Parent { get; set; }

        public override IEnumerable<SyntaxNode> GetChildren() =>
            new[] { VariableDeclaration != null ? (SyntaxNode)VariableDeclaration : Expression }.Concat(Statements);

        public override void ReplaceExpressions<T>(Func<T, bool> filter, Func<T, Expression> projection)
        {
            VariableDeclaration?.ReplaceExpressions(filter, projection);

            Expression = Expression.ReplaceExpressions(Expression, filter, projection);

            foreach (var statement in Statements)
            {
                statement.ReplaceExpressions(filter, projection);
            }
        }
    }
}