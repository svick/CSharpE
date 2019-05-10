using System;
using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RoslynSyntaxFactory = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Roslyn = Microsoft.CodeAnalysis;

namespace CSharpE.Syntax
{
    public sealed class LabelStatement : Statement
    {
        private LabeledStatementSyntax syntax;

        internal LabelStatement(LabeledStatementSyntax syntax, SyntaxNode parent)
            : base(syntax)
        {
            Init(syntax);
            Parent = parent;
        }

        private void Init(LabeledStatementSyntax syntax)
        {
            this.syntax = syntax;
            name = new Identifier(syntax.Identifier);
        }

        public LabelStatement(string name, Statement statement)
        {
            this.name = new Identifier(name);
            Statement = statement;
        }

        private Identifier name;
        public string Name
        {
            get => name.Text;
            set => name.Text = value;
        }

        private Statement statement;
        public Statement Statement
        {
            get
            {
                if (statement == null)
                    statement = FromRoslyn.Statement(syntax.Statement, this);

                return statement;
            }
            set => SetNotNull(ref statement, value);
        }

        private protected override StatementSyntax GetWrappedStatement(ref bool? changed)
        {
            GetAndResetChanged(ref changed);

            bool? thisChanged = false;

            var newName = name.GetWrapped(ref thisChanged);
            var newStatement = statement?.GetWrapped(ref thisChanged) ?? syntax.Statement;

            if (syntax == null || thisChanged == true)
            {
                syntax = RoslynSyntaxFactory.LabeledStatement(newName, newStatement);

                SetChanged(ref changed);
            }

            return syntax;
        }

        private protected override void SetSyntaxImpl(Roslyn::SyntaxNode newSyntax)
        {
            Init((LabeledStatementSyntax)newSyntax);
            Set(ref statement, null);
        }

        private protected override SyntaxNode CloneImpl() => new LabelStatement(Name, Statement);

        public override void ReplaceExpressions<T>(Func<T, bool> filter, Func<T, Expression> projection) =>
            Statement.ReplaceExpressions(filter, projection);
    }
}