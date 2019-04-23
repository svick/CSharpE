using System;
using System.Collections.Generic;
using System.Linq;
using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RoslynSyntaxFactory = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Roslyn = Microsoft.CodeAnalysis;

namespace CSharpE.Syntax
{
    public sealed class BlockStatement : Statement
    {
        private BlockSyntax syntax;

        internal BlockStatement(BlockSyntax syntax, SyntaxNode parent)
        {
            this.syntax = syntax;
            Parent = parent;
        }

        public BlockStatement(params Statement[] statements) : this(statements.AsEnumerable()) { }

        public BlockStatement(IEnumerable<Statement> statements) =>
            this.statements = new StatementList(statements, this);

        private StatementList statements;
        public IList<Statement> Statements
        {
            get
            {
                if (statements == null)
                    statements = new StatementList(syntax.Statements, this);

                return statements;
            }
            set => SetList(ref statements, new StatementList(value, this));
        }

        internal BlockSyntax GetWrapped(ref bool? changed)
        {
            GetAndResetChanged(ref changed);

            bool? thisChanged = false;

            var newStatements = statements?.GetWrapped(ref thisChanged) ?? syntax.Statements;

            if (syntax == null || thisChanged == true)
            {
                syntax = RoslynSyntaxFactory.Block(newStatements);

                SetChanged(ref changed);
            }

            return syntax;
        }

        private protected override StatementSyntax GetWrappedStatement(ref bool? changed) => GetWrapped(ref changed);

        private protected override void SetSyntaxImpl(Roslyn::SyntaxNode newSyntax)
        {
            SetList(ref statements, null);
            syntax = (BlockSyntax)newSyntax;
        }

        internal override SyntaxNode Clone() => new BlockStatement(Statements);

        internal override SyntaxNode Parent { get; set; }

        public override IEnumerable<SyntaxNode> GetChildren() => Statements;

        public override void ReplaceExpressions<T>(Func<T, bool> filter, Func<T, Expression> projection)
        {
            foreach (var statement in Statements)
            {
                statement.ReplaceExpressions(filter, projection);
            }
        }
    }
}