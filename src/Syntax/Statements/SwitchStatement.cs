using System;
using System.Collections.Generic;
using System.Linq;
using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RoslynSyntaxFactory = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Roslyn = Microsoft.CodeAnalysis;

namespace CSharpE.Syntax
{
    public sealed class SwitchStatement : Statement
    {
        private SwitchStatementSyntax syntax;

        internal SwitchStatement(SwitchStatementSyntax syntax, SyntaxNode parent) : base(syntax)
        {
            this.syntax = syntax;
            Parent = parent;
        }

        public SwitchStatement(Expression expression, params SwitchSection[] sections)
            : this(expression, sections.AsEnumerable()) { }

        public SwitchStatement(Expression expression, IEnumerable<SwitchSection> sections)
        {
            Expression = expression;
            Sections = sections?.ToList();
        }

        private Expression expression;
        public Expression Expression
        {
            get
            {
                if (expression == null)
                    expression = FromRoslyn.Expression(syntax.Expression, this);

                return expression;
            }
            set => SetNotNull(ref expression, value);
        }

        private SyntaxList<SwitchSection, SwitchSectionSyntax> sections;
        public IList<SwitchSection> Sections
        {
            get
            {
                if (sections == null)
                    sections = new SyntaxList<SwitchSection, SwitchSectionSyntax>(syntax.Sections, this);

                return sections;
            }
            set => SetList(ref sections, new SyntaxList<SwitchSection, SwitchSectionSyntax>(value, this));
        }

        private protected override StatementSyntax GetWrappedStatement(ref bool? changed)
        {
            GetAndResetChanged(ref changed, out var thisChanged);

            var newExpression = expression?.GetWrapped(ref thisChanged) ?? syntax.Expression;
            var newSections = sections?.GetWrapped(ref thisChanged) ?? syntax.Sections;

            if (syntax == null || thisChanged == true || ShouldAnnotate(syntax, changed))
            {
                syntax = RoslynSyntaxFactory.SwitchStatement(newExpression, newSections);

                syntax = Annotate(syntax);

                SetChanged(ref changed);
            }

            return syntax;
        }

        private protected override void SetSyntaxImpl(Roslyn::SyntaxNode newSyntax)
        {
            syntax = (SwitchStatementSyntax)newSyntax;

            Set(ref expression, null);
            SetList(ref sections, null);
        }

        private protected override SyntaxNode CloneImpl() => new SwitchStatement(Expression, Sections);

        public override IEnumerable<SyntaxNode> GetChildren() => new SyntaxNode[] { Expression }.Concat(Sections);

        public override void ReplaceExpressions<T>(Func<T, bool> filter, Func<T, Expression> projection)
        {
            Expression = Expression.ReplaceExpressions(Expression, filter, projection);

            foreach (var section in Sections)
            {
                section.ReplaceExpressions(filter, projection);
            }
        }
    }
}