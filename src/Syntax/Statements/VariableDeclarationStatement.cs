using System;
using System.Diagnostics;
using System.Linq;
using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RoslynSyntaxFactory = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Roslyn = Microsoft.CodeAnalysis;

namespace CSharpE.Syntax
{
    public sealed class VariableDeclarationStatement : Statement
    {
        private LocalDeclarationStatementSyntax syntax;

        internal VariableDeclarationStatement(LocalDeclarationStatementSyntax syntax, SyntaxNode parent)
            : base(syntax)
        {
            Debug.Assert(syntax.Declaration.Variables.Count == 1);
            Debug.Assert(syntax.Declaration.Variables.Single().ArgumentList == null);

            Init(syntax);
            Parent = parent;
        }

        private void Init(LocalDeclarationStatementSyntax syntax)
        {
            this.syntax = syntax;

            IsConst = syntax.IsConst;
            name = new Identifier(syntax.Declaration.Variables.Single().Identifier);
        }

        public VariableDeclarationStatement(TypeReference type, string name, Expression initializer = null)
            : this(false, type, name, initializer) { }

        public VariableDeclarationStatement(bool isConst, TypeReference type, string name, Expression initializer = null)
        {
            IsConst = isConst;
            Type = type;
            Name = name;
            Initializer = initializer;
        }

        public bool IsConst { get; set; }

        private TypeReference type;
        public TypeReference Type
        {
            get
            {
                if (type == null)
                    type = FromRoslyn.TypeReference(syntax.Declaration.Type, this);

                return type;
            }
            set => SetNotNull(ref type, value);
        }

        private Identifier name;
        public string Name
        {
            get => name.Text;
            set => name.Text = value;
        }

        private bool initializerSet;
        private Expression initializer;
        public Expression Initializer
        {
            get
            {
                if (!initializerSet)
                {
                    initializer = FromRoslyn.Expression(syntax.Declaration.Variables.Single().Initializer?.Value, this);
                    initializerSet = true;
                }

                return initializer;
            }
            set
            {
                Set(ref initializer, value);
                initializerSet = true;
            }
        }


        internal LocalDeclarationStatementSyntax GetWrapped(ref bool? changed)
        {
            GetAndResetChanged(ref changed, out var thisChanged);

            var newType = type?.GetWrapped(ref thisChanged) ?? syntax.Declaration.Type;
            var newName = name.GetWrapped(ref thisChanged);
            var newInitializer = initializerSet ? initializer?.GetWrapped(ref thisChanged) : syntax.Declaration.Variables.Single().Initializer?.Value;

            if (syntax == null || thisChanged == true || IsConst != syntax.IsConst)
            {
                var modifiers = IsConst
                    ? RoslynSyntaxFactory.TokenList(RoslynSyntaxFactory.Token(SyntaxKind.ConstKeyword))
                    : RoslynSyntaxFactory.TokenList();

                var equalsClause = newInitializer == null
                    ? null
                    : RoslynSyntaxFactory.EqualsValueClause(newInitializer);

                syntax = RoslynSyntaxFactory.LocalDeclarationStatement(
                    modifiers,
                    RoslynSyntaxFactory.VariableDeclaration(
                        newType,
                        RoslynSyntaxFactory.SingletonSeparatedList(
                            RoslynSyntaxFactory.VariableDeclarator(newName, default, equalsClause))));

                SetChanged(ref changed);
            }

            return syntax;
        }

        private protected override StatementSyntax GetWrappedStatement(ref bool? changed) => GetWrapped(ref changed);

        private protected override void SetSyntaxImpl(Roslyn::SyntaxNode newSyntax)
        {
            syntax = (LocalDeclarationStatementSyntax)newSyntax;
            Set(ref type, null);
            initializerSet = false;
            Set(ref initializer, null);
        }

        private protected override SyntaxNode CloneImpl() => new VariableDeclarationStatement(IsConst, Type, Name, Initializer);

        public override void ReplaceExpressions<T>(Func<T, bool> filter, Func<T, Expression> projection) =>
            Initializer = Expression.ReplaceExpressions(Initializer, filter, projection);
    }
}