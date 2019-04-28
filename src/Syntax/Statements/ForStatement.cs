using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RoslynSyntaxFactory = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Roslyn = Microsoft.CodeAnalysis;

namespace CSharpE.Syntax
{
    public sealed class ForStatement : Statement
    {
        private ForStatementSyntax syntax;

        internal ForStatement(ForStatementSyntax syntax, SyntaxNode parent) : base(syntax)
        {
            Debug.Assert(syntax.Initializers == default);
            Debug.Assert(syntax.Declaration == null || syntax.Declaration.Variables.Count == 1);

            this.syntax = syntax;
            Parent = parent;
        }

        public ForStatement(
            VariableDeclarationStatement variableDeclaration, Expression condition, Expression incrementor,
            params Statement[] statements)
            : this(variableDeclaration, condition, new[] { incrementor }, statements) { }

        public ForStatement(
            VariableDeclarationStatement variableDeclaration, Expression condition,
            IEnumerable<Expression> incrementors, IEnumerable<Statement> statements)
        {
            VariableDeclaration = variableDeclaration;
            Condition = condition;
            this.incrementors = new ExpressionList(incrementors, this);
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
            }
        }

        private Expression condition;
        public Expression Condition
        {
            get
            {
                if (condition == null)
                    condition = FromRoslyn.Expression(syntax.Condition, this);

                return condition;
            }
            set => SetNotNull(ref condition, value);
        }

        private ExpressionList incrementors;
        public IList<Expression> Incrementors
        {
            get
            {
                if (incrementors == null)
                    incrementors = new ExpressionList(syntax.Incrementors, this);

                return incrementors;
            }
            set => SetList(ref incrementors, new ExpressionList(value, this));
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

            var newVariableDeclaration = variableDeclaration?.GetWrapped(ref thisChanged).Declaration ?? syntax.Declaration;
            var newCondition = condition?.GetWrapped(ref thisChanged) ?? syntax.Condition;
            var newIncrementors = incrementors?.GetWrapped(ref thisChanged) ?? syntax.Incrementors;
            var newStatements = statements?.GetWrapped(ref thisChanged) ?? GetStatementList(syntax.Statement);

            if (syntax == null || thisChanged == true)
            {
                syntax = RoslynSyntaxFactory.ForStatement(
                    newVariableDeclaration, default, newCondition, newIncrementors,
                    RoslynSyntaxFactory.Block(newStatements));

                SetChanged(ref changed);
            }

            return syntax;
        }

        private protected override void SetSyntaxImpl(Roslyn::SyntaxNode newSyntax)
        {
            syntax = (ForStatementSyntax)newSyntax;

            Set(ref variableDeclaration, null);
            Set(ref condition, null);
            SetList(ref statements, null);
        }

        internal override SyntaxNode Clone() => new ForStatement(VariableDeclaration, Condition, Incrementors, Statements);

        internal override SyntaxNode Parent { get; set; }

        public override IEnumerable<SyntaxNode> GetChildren() =>
            new SyntaxNode[] { VariableDeclaration, Condition }.Concat(Incrementors).Concat(Statements);

        public override void ReplaceExpressions<T>(Func<T, bool> filter, Func<T, Expression> projection)
        {
            foreach (var statement in Statements)
            {
                statement.ReplaceExpressions(filter, projection);
            }

            for (var i = 0; i < Incrementors.Count; i++)
            {
                Incrementors[i] = Expression.ReplaceExpressions(Incrementors[i], filter, projection);
            }

            VariableDeclaration.ReplaceExpressions(filter, projection);
            Condition = Expression.ReplaceExpressions(Condition, filter, projection);
        }
    }
}