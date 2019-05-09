using System;
using System.Collections.Generic;
using System.Linq;
using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RoslynSyntaxFactory = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Roslyn = Microsoft.CodeAnalysis;

namespace CSharpE.Syntax
{
    public sealed class CatchClause : SyntaxNode, ISyntaxWrapper<CatchClauseSyntax>
    {
        private CatchClauseSyntax syntax;

        internal CatchClause(CatchClauseSyntax syntax, TryStatement parent)
            : base(syntax)
        {
            Init(syntax);
            Parent = parent;
        }

        private void Init(CatchClauseSyntax syntax)
        {
            this.syntax = syntax;
            exceptionName = new Identifier(syntax.Declaration?.Identifier);
        }

        public CatchClause(TypeReference exceptionType, string exceptionName, params Statement[] statements)
            : this(exceptionType, exceptionName, statements.AsEnumerable()) { }

        public CatchClause(TypeReference exceptionType, string exceptionName, IEnumerable<Statement> statements)
            : this(exceptionType, exceptionName, null, statements) { }

        public CatchClause(
            TypeReference exceptionType, string exceptionName, Expression filter, IEnumerable<Statement> statements)
        {
            ExceptionType = exceptionType;
            this.exceptionName = new Identifier(exceptionName, true);
            Filter = filter;
            this.statements = new StatementList(statements, this);
        }

        private bool exceptionTypeSet;
        private TypeReference exceptionType;
        public TypeReference ExceptionType
        {
            get
            {
                if (!exceptionTypeSet)
                {
                    exceptionType = FromRoslyn.TypeReference(syntax.Declaration?.Type, this);
                    exceptionTypeSet = true;
                }

                return exceptionType;
            }
            set
            {
                if (value == null && ExceptionName != null)
                    throw new ArgumentException(
                        "Can't set ExceptionType to null while ExceptionName is not null.", nameof(value));

                Set(ref exceptionType, value);
                exceptionTypeSet = true;
            }
        }

        private Identifier exceptionName;
        public string ExceptionName
        {
            get => exceptionName.Text;
            set
            {
                // PERF: assigning non-null should not require creating ExceptionType
                if (value != null && ExceptionType == null)
                    throw new ArgumentException(
                        "Can't set ExceptionName to not null while ExceptionType is null.", nameof(value));

                exceptionName.Text = value;
            }
        }

        private bool filterSet;
        private Expression filter;
        public Expression Filter
        {
            get
            {
                if (!filterSet)
                {
                    filter = FromRoslyn.Expression(syntax.Filter?.FilterExpression, this);
                    filterSet = true;
                }

                return filter;
            }
            set
            {
                Set(ref filter, value);
                filterSet = true;
            }
        }

        private StatementList statements;
        public IList<Statement> Statements
        {
            get
            {
                if (statements == null)
                    statements = new StatementList(syntax.Block.Statements, this);

                return statements;
            }
            set => SetList(ref statements, new StatementList(value, this));
        }

        CatchClauseSyntax ISyntaxWrapper<CatchClauseSyntax>.GetWrapped(ref bool? changed)
        {
            GetAndResetChanged(ref changed);

            bool? thisChanged = false;

            var newExceptionType =
                exceptionTypeSet ? exceptionType?.GetWrapped(ref thisChanged) : syntax.Declaration?.Type;
            var newExceptionName = exceptionName.GetWrapped(ref thisChanged);
            var newFilter = filterSet ? filter?.GetWrapped(ref thisChanged) : syntax.Filter?.FilterExpression;
            var newStatements = statements?.GetWrapped(ref thisChanged) ?? syntax.Block.Statements;

            if (syntax == null || thisChanged == true)
            {
                var declarationSyntax = newExceptionType == null
                    ? null
                    : RoslynSyntaxFactory.CatchDeclaration(newExceptionType, newExceptionName);

                var filterSyntax = newFilter == null ? null : RoslynSyntaxFactory.CatchFilterClause(newFilter);

                syntax = RoslynSyntaxFactory.CatchClause(
                    declarationSyntax, filterSyntax, RoslynSyntaxFactory.Block(newStatements));

                SetChanged(ref changed);
            }

            return syntax;
        }

        private protected override void SetSyntaxImpl(Roslyn::SyntaxNode newSyntax)
        {
            Set(ref exceptionType, null);
            exceptionTypeSet = false;
            Set(ref filter, null);
            filterSet = false;
            SetList(ref statements, null);

            Init((CatchClauseSyntax)newSyntax);
        }

        private protected override SyntaxNode CloneImpl() => new CatchClause(ExceptionType, ExceptionName, Filter, Statements);
    }
}