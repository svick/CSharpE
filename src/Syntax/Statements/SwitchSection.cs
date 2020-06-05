using System;
using System.Collections.Generic;
using System.Linq;
using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RoslynSyntaxFactory = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Roslyn = Microsoft.CodeAnalysis;

namespace CSharpE.Syntax
{
    public sealed class SwitchSection : SyntaxNode, ISyntaxWrapper<SwitchSectionSyntax>
    {
        private SwitchSectionSyntax syntax;

        internal SwitchSection(SwitchSectionSyntax syntax, SyntaxNode parent) : base(syntax)
        {
            this.syntax = syntax;
            Parent = parent;
        }

        public SwitchSection(SwitchLabel label, params Statement[] statements)
            : this(label, statements.AsEnumerable()) { }

        public SwitchSection(SwitchLabel label, IEnumerable<Statement> statements)
            : this(new[] { label }, statements) { }

        public SwitchSection(IEnumerable<SwitchLabel> labels, params Statement[] statements)
            : this(labels, statements.AsEnumerable()) { }

        public SwitchSection(IEnumerable<SwitchLabel> labels, IEnumerable<Statement> statements)
        {
            this.labels = new SwitchLabelList(labels, this);
            this.statements = new StatementList(statements, this);
        }

        private SwitchLabelList labels;
        public IList<SwitchLabel> Labels
        {
            get
            {
                if (labels == null)
                    labels = new SwitchLabelList(syntax.Labels, this);

                return labels;
            }
            set => SetList(ref labels, new SwitchLabelList(value, this));
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

        internal SwitchSectionSyntax GetWrapped(ref bool? changed)
        {
            GetAndResetChanged(ref changed, out var thisChanged);

            var newLabels = labels?.GetWrapped(ref thisChanged) ?? syntax.Labels;
            var newStatements = statements?.GetWrapped(ref thisChanged) ?? syntax.Statements;

            if (syntax == null || thisChanged == true || ShouldAnnotate(syntax, changed))
            {
                syntax = RoslynSyntaxFactory.SwitchSection(newLabels, newStatements);

                syntax = Annotate(syntax);

                SetChanged(ref changed);
            }

            return syntax;
        }

        SwitchSectionSyntax ISyntaxWrapper<SwitchSectionSyntax>.GetWrapped(ref bool? changed) => GetWrapped(ref changed);

        private protected override void SetSyntaxImpl(Roslyn::SyntaxNode newSyntax)
        {
            syntax = (SwitchSectionSyntax)newSyntax;

            SetList(ref labels, null);
            SetList(ref statements, null);
        }

        private protected override SyntaxNode CloneImpl() => new SwitchSection(Labels, Statements);

        public override IEnumerable<SyntaxNode> GetChildren() => Labels.Concat<SyntaxNode>(Statements);

        public void ReplaceExpressions<T>(Func<T, bool> filter, Func<T, Expression> projection) where T : Expression
        {
            foreach (var statement in Statements)
            {
                statement.ReplaceExpressions(filter, projection);
            }
        }
    }
}