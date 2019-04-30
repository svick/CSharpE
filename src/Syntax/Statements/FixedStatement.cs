using System;
using System.Collections.Generic;
using System.Linq;
using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RoslynSyntaxFactory = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Roslyn = Microsoft.CodeAnalysis;

namespace CSharpE.Syntax
{
    public sealed class FixedStatement : Statement
    {
        private FixedStatementSyntax syntax;

        internal FixedStatement(FixedStatementSyntax syntax, SyntaxNode parent) : base(syntax)
        {
            this.syntax = syntax;
            Parent = parent;
        }

        public FixedStatement(VariableDeclarationStatement variableDeclaration, params Statement[] statements)
            : this(variableDeclaration, statements.AsEnumerable()) { }

        public FixedStatement(VariableDeclarationStatement variableDeclaration, IEnumerable<Statement> statements)
        {
            VariableDeclaration = variableDeclaration;
            this.statements = new StatementList(statements, this);
        }

        private VariableDeclarationStatement variableDeclaration;
        public VariableDeclarationStatement VariableDeclaration
        {
            get
            {
                if (variableDeclaration != null)
                    variableDeclaration = new VariableDeclarationStatement(
                        RoslynSyntaxFactory.LocalDeclarationStatement(syntax.Declaration), this);

                return variableDeclaration;
            }
            set => SetNotNull(ref variableDeclaration, value);
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
            var newStatements = statements?.GetWrapped(ref thisChanged) ?? GetStatementList(syntax.Statement);

            if (syntax == null || thisChanged == true)
            {
                syntax = RoslynSyntaxFactory.FixedStatement(newVariable, RoslynSyntaxFactory.Block(newStatements));

                SetChanged(ref changed);
            }

            return syntax;
        }

        private protected override void SetSyntaxImpl(Roslyn::SyntaxNode newSyntax)
        {
            syntax = (FixedStatementSyntax)newSyntax;

            Set(ref variableDeclaration, null);
            SetList(ref statements, null);
        }

        internal override SyntaxNode Clone() => new FixedStatement(VariableDeclaration, Statements);

        internal override SyntaxNode Parent { get; set; }

        public override IEnumerable<SyntaxNode> GetChildren() =>
            new SyntaxNode[] { VariableDeclaration }.Concat(Statements);

        public override void ReplaceExpressions<T>(Func<T, bool> filter, Func<T, Expression> projection)
        {
            VariableDeclaration?.ReplaceExpressions(filter, projection);

            foreach (var statement in Statements)
            {
                statement.ReplaceExpressions(filter, projection);
            }
        }
    }
}