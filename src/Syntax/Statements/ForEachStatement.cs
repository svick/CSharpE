using System;
using System.Collections.Generic;
using System.Linq;
using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RoslynSyntaxFactory = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Roslyn = Microsoft.CodeAnalysis;

namespace CSharpE.Syntax
{
    public abstract class BaseForEachStatement : Statement
    {
        private protected abstract CommonForEachStatementSyntax Syntax { get; }

        protected BaseForEachStatement() { }
        protected BaseForEachStatement(CommonForEachStatementSyntax syntax) : base(syntax) { }

        private protected Expression expression;
        public Expression Expression
        {
            get
            {
                if (expression == null)
                    expression = FromRoslyn.Expression(Syntax.Expression, this);

                return expression;
            }
            set => SetNotNull(ref expression, value);
        }

        private protected StatementList statements;
        public IList<Statement> Statements
        {
            get
            {
                if (statements == null)
                    statements = new StatementList(GetStatementList(Syntax.Statement), this);

                return statements;
            }
            set => SetList(ref statements, new StatementList(value, this));
        }

        internal override SyntaxNode Parent { get; set; }
    }

    public sealed class ForEachStatement : BaseForEachStatement
    {
        private ForEachStatementSyntax syntax;

        private protected override CommonForEachStatementSyntax Syntax => syntax;

        internal ForEachStatement(ForEachStatementSyntax syntax, SyntaxNode parent) : base(syntax)
        {
            Init(syntax);
            Parent = parent;
        }

        private void Init(ForEachStatementSyntax syntax)
        {
            this.syntax = syntax;

            elementName = new Identifier(syntax.Identifier);
        }

        public ForEachStatement(
            TypeReference elementType, string elementName, Expression expression, params Statement[] statements)
            : this(elementType, elementName, expression, statements.AsEnumerable()) { }

        public ForEachStatement(
            TypeReference elementType, string elementName, Expression expression, IEnumerable<Statement> statements)
        {
            ElementType = elementType;
            ElementName = elementName;
            Expression = expression;
            this.statements = new StatementList(statements, this);
        }

        private TypeReference elementType;
        public TypeReference ElementType
        {
            get
            {
                if (elementType == null)
                    elementType = FromRoslyn.TypeReference(syntax.Type, this);

                return elementType;
            }
            set => SetNotNull(ref elementType, value);
        }

        private Identifier elementName;
        public string ElementName
        {
            get => elementName.Text;
            set => elementName.Text = value;
        }

        private protected override StatementSyntax GetWrappedStatement(ref bool? changed)
        {
            GetAndResetChanged(ref changed);

            bool? thisChanged = false;

            var newType = elementType?.GetWrapped(ref thisChanged) ?? syntax.Type;
            var newName = elementName.GetWrapped(ref thisChanged);
            var newExpression = expression?.GetWrapped(ref thisChanged) ?? syntax.Expression;
            var newStatements = statements?.GetWrapped(ref thisChanged) ?? GetStatementList(syntax.Statement);

            if (syntax == null || thisChanged == true)
            {
                syntax = RoslynSyntaxFactory.ForEachStatement(
                    newType, newName, newExpression, RoslynSyntaxFactory.Block(newStatements));

                SetChanged(ref changed);
            }

            return syntax;
        }

        private protected override void SetSyntaxImpl(Roslyn::SyntaxNode newSyntax)
        {
            Init((ForEachStatementSyntax)newSyntax);

            Set(ref elementType, null);
            Set(ref expression, null);
            SetList(ref statements, null);
        }

        private protected override SyntaxNode CloneImpl() => new ForEachStatement(ElementType, ElementName, Expression, Statements);

        public override IEnumerable<SyntaxNode> GetChildren() =>
            new SyntaxNode[] { ElementType, Expression }.Concat(Statements);

        public override void ReplaceExpressions<T>(Func<T, bool> filter, Func<T, Expression> projection)
        {
            foreach (var statement in Statements)
            {
                statement.ReplaceExpressions(filter, projection);
            }

            Expression = Expression.ReplaceExpressions(Expression, filter, projection);
        }
    }

    public sealed class PatternForEachStatement : BaseForEachStatement
    {
        private ForEachVariableStatementSyntax syntax;

        private protected override CommonForEachStatementSyntax Syntax => syntax;

        internal PatternForEachStatement(ForEachVariableStatementSyntax syntax, SyntaxNode parent) : base(syntax)
        {
            this.syntax = syntax;
            Parent = parent;
        }

        public PatternForEachStatement(
            Expression elementPattern, Expression expression, params Statement[] statements)
            : this(elementPattern, expression, statements.AsEnumerable()) { }

        public PatternForEachStatement(
            Expression elementPattern, Expression expression, IEnumerable<Statement> statements)
        {
            ElementPattern = elementPattern;
            Expression = expression;
            this.statements = new StatementList(statements, this);
        }

        private Expression elementPattern;
        public Expression ElementPattern
        {
            get
            {
                if (elementPattern == null)
                    elementPattern = FromRoslyn.Expression(syntax.Variable, this);

                return elementPattern;
            }
            set => SetNotNull(ref elementPattern, value);
        }

        private protected override StatementSyntax GetWrappedStatement(ref bool? changed)
        {
            GetAndResetChanged(ref changed);

            bool? thisChanged = false;

            var newPattern = elementPattern?.GetWrapped(ref thisChanged) ?? syntax.Variable;
            var newExpression = expression?.GetWrapped(ref thisChanged) ?? syntax.Expression;
            var newStatements = statements?.GetWrapped(ref thisChanged) ?? GetStatementList(syntax.Statement);

            if (syntax == null || thisChanged == true)
            {
                syntax = RoslynSyntaxFactory.ForEachVariableStatement(
                    newPattern, newExpression, RoslynSyntaxFactory.Block(newStatements));

                SetChanged(ref changed);
            }

            return syntax;
        }

        private protected override void SetSyntaxImpl(Roslyn::SyntaxNode newSyntax)
        {
            this.syntax = (ForEachVariableStatementSyntax)newSyntax;

            Set(ref elementPattern, null);
            Set(ref expression, null);
            SetList(ref statements, null);
        }

        private protected override SyntaxNode CloneImpl() => new PatternForEachStatement(ElementPattern, Expression, Statements);

        public override IEnumerable<SyntaxNode> GetChildren() =>
            new SyntaxNode[] { ElementPattern, Expression }.Concat(Statements);

        public override void ReplaceExpressions<T>(Func<T, bool> filter, Func<T, Expression> projection)
        {
            foreach (var statement in Statements)
            {
                statement.ReplaceExpressions(filter, projection);
            }

            ElementPattern = Expression.ReplaceExpressions(ElementPattern, filter, projection);

            Expression = Expression.ReplaceExpressions(Expression, filter, projection);
        }
    }
}