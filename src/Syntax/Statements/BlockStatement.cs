using System.Collections.Generic;
using System.Linq;
using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RoslynSyntaxFactory = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Roslyn = Microsoft.CodeAnalysis;

namespace CSharpE.Syntax
{
    public sealed class BlockStatement : Statement, ISyntaxWrapper<BlockSyntax>
    {
        private BlockSyntax syntax;

        internal BlockStatement(BlockSyntax syntax, SyntaxNode parent)
        {
            this.syntax = syntax;
            Parent = parent;
        }

        public BlockStatement(IEnumerable<Statement> statements)
        {
            Statements = statements.ToList();
        }

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

        BlockSyntax ISyntaxWrapper<BlockSyntax>.GetWrapped(ref bool? changed)
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

        private protected override StatementSyntax GetWrappedStatement(ref bool? changed) =>
            this.GetWrapped<BlockSyntax>(ref changed);

        internal BlockSyntax GetWrapped(ref bool? changed) => this.GetWrapped<BlockSyntax>(ref changed);

        private protected override void SetSyntaxImpl(Roslyn::SyntaxNode newSyntax)
        {
            syntax = (BlockSyntax)newSyntax;

            statements = null;
        }

        internal override SyntaxNode Clone()
        {
            if (statements == null)
            {
                return new BlockStatement(syntax, null);
            }

            return new BlockStatement(Statements);
        }

        internal override SyntaxNode Parent { get; set; }

        internal override IEnumerable<SyntaxNode> GetChildren() => Statements;
    }
}