using System;
using System.Collections.Generic;
using System.Linq;
using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static CSharpE.Syntax.MemberModifiers;
using RoslynSyntaxFactory = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Roslyn = Microsoft.CodeAnalysis;

namespace CSharpE.Syntax
{
    public sealed class ConstructorDefinition : BaseMethodDefinition, ISyntaxWrapper<ConstructorDeclarationSyntax>
    {
        private ConstructorDeclarationSyntax syntax;

        private protected override BaseMethodDeclarationSyntax BaseMethodSyntax => syntax;

        internal ConstructorDefinition(ConstructorDeclarationSyntax syntax, TypeDefinition parent)
            : base(syntax)
        {
            Init(syntax);
            Parent = parent;
        }

        private void Init(ConstructorDeclarationSyntax syntax)
        {
            this.syntax = syntax;

            Modifiers = FromRoslyn.MemberModifiers(syntax.Modifiers);
        }

        public ConstructorDefinition(
            MemberModifiers modifiers, IEnumerable<Parameter> parameters, BlockStatement statementBody)
            : this(modifiers, parameters, null, statementBody) { }

        public ConstructorDefinition(
            MemberModifiers modifiers, IEnumerable<Parameter> parameters, ConstructorInitializer initializer, BlockStatement statementBody)
            : this(modifiers, parameters, initializer, statementBody, null) { }

        public ConstructorDefinition(
            MemberModifiers modifiers, IEnumerable<Parameter> parameters, Expression expressionBody)
            : this(modifiers, parameters, null, expressionBody) { }

        public ConstructorDefinition(
            MemberModifiers modifiers, IEnumerable<Parameter> parameters, ConstructorInitializer initializer, Expression expressionBody)
            : this(modifiers, parameters, initializer, null, expressionBody) { }

        private ConstructorDefinition(
            MemberModifiers modifiers, IEnumerable<Parameter> parameters, ConstructorInitializer initializer,
            BlockStatement statementBody, Expression expressionBody)
        {
            Modifiers = modifiers;
            Parameters = parameters?.ToList();
            Initializer = initializer;
            StatementBody = statementBody;
            ExpressionBody = expressionBody;
        }

        #region Modifiers

        private const MemberModifiers ValidMethodModifiers =
            AccessModifiersMask | Extern | Unsafe | Static;

        private protected override void ValidateModifiers(MemberModifiers value)
        {
            var invalidModifiers = value & ~ValidMethodModifiers;
            if (invalidModifiers != 0)
                throw new ArgumentException($"The modifiers {invalidModifiers} are not valid for a constructor.", nameof(value));
        }

        public bool IsStatic
        {
            get => Modifiers.Contains(Static);
            set => Modifiers = Modifiers.With(Static, value);
        }

        #endregion

        private bool initializerSet;
        private ConstructorInitializer initializer;
        public ConstructorInitializer Initializer
        {
            get
            {
                if (!initializerSet)
                {
                    initializer = syntax.Initializer == null
                        ? null
                        : new ConstructorInitializer(syntax.Initializer, this);
                    initializerSet = true;
                }

                return initializer;
            }
            set
            {
                initializer = value;
                initializerSet = true;
            }
        }

        ConstructorDeclarationSyntax ISyntaxWrapper<ConstructorDeclarationSyntax>.GetWrapped(ref bool? changed)
        {
            GetAndResetChanged(ref changed, out var thisChanged);

            var newAttributes = attributes?.GetWrapped(ref thisChanged) ?? syntax?.AttributeLists ?? default;
            var newModifiers = Modifiers;
            var newParameters = parameters?.GetWrapped(ref thisChanged) ?? syntax.ParameterList.Parameters;
            var newInitializer = initializerSet ? initializer?.GetWrapped(ref thisChanged) : syntax.Initializer;
            var newStatementBody = statementBodySet ? statementBody?.GetWrapped(ref thisChanged) : syntax.Body;
            var newExpressionBody = expressionBodySet ? expressionBody?.GetWrapped(ref thisChanged) : syntax.ExpressionBody?.Expression;

            if (syntax == null || newModifiers != FromRoslyn.MemberModifiers(syntax.Modifiers) ||
                thisChanged == true || ShouldAnnotate(syntax, changed))
            {
                if (Parent == null)
                    throw new InvalidOperationException("Can't create syntax node for constructor with no parent type.");

                var arrowClause = newExpressionBody == null ? null : RoslynSyntaxFactory.ArrowExpressionClause(newExpressionBody);
                var semicolonToken = newStatementBody == null ? RoslynSyntaxFactory.Token(SyntaxKind.SemicolonToken) : default;

                var newSyntax = RoslynSyntaxFactory.ConstructorDeclaration(
                    newAttributes, newModifiers.GetWrapped(), RoslynSyntaxFactory.Identifier(ParentType.Name),
                    RoslynSyntaxFactory.ParameterList(newParameters), newInitializer, newStatementBody, arrowClause, semicolonToken);

                syntax = Annotate(newSyntax);

                SetChanged(ref changed);
            }

            return syntax;
        }

        private protected override BaseMethodDeclarationSyntax GetWrappedBaseMethod(ref bool? changed) =>
            this.GetWrapped<ConstructorDeclarationSyntax>(ref changed);

        private protected override void SetSyntaxImpl(Roslyn::SyntaxNode newSyntax)
        {
            Init((ConstructorDeclarationSyntax)newSyntax);

            SetList(ref attributes, null);
            SetList(ref parameters, null);
            Set(ref initializer, null);
            initializerSet = false;
            Set(ref statementBody, null);
            statementBodySet = false;
            Set(ref expressionBody, null);
            expressionBodySet = false;
        }

        private protected override SyntaxNode CloneImpl() =>
            new ConstructorDefinition(Modifiers, Parameters, Initializer, StatementBody, ExpressionBody) { Attributes = Attributes };

        protected override void ReplaceExpressionsImpl<T>(Func<T, bool> filter, Func<T, Expression> projection)
        {
            Initializer?.ReplaceExpressions(filter, projection);

            base.ReplaceExpressionsImpl(filter, projection);
        }

        public override IEnumerable<SyntaxNode> GetChildren() =>
            Attributes.Concat<SyntaxNode>(Parameters).Concat(new SyntaxNode[] { Initializer, StatementBody, ExpressionBody });
    }
}