using System;
using System.Linq;
using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static CSharpE.Syntax.MemberModifiers;
using RoslynSyntaxFactory = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Roslyn = Microsoft.CodeAnalysis;

namespace CSharpE.Syntax
{
    public sealed class FieldDefinition : MemberDefinition, ISyntaxWrapper<FieldDeclarationSyntax>
    {
        private const MemberModifiers ValidModifiers =
            AccessModifiersMask | New | Static | Unsafe | Const | ReadOnly | Volatile;

        private protected override void ValidateModifiers(MemberModifiers value)
        {
            var invalidModifiers = value & ~ValidModifiers;
            if (invalidModifiers != 0)
                throw new ArgumentException($"The modifiers {invalidModifiers} are not valid for a field.", nameof(value));
        }

        private FieldDeclarationSyntax syntax;

        private protected override MemberDeclarationSyntax MemberSyntax => syntax;

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
                    var syntaxInitializer = syntax.Declaration.Variables.Single().Initializer;

                    if (syntaxInitializer != null)
                        initializer = FromRoslyn.Expression(syntaxInitializer.Value, this);

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

        internal FieldDefinition(FieldDeclarationSyntax syntax, TypeDefinition parent)
        {
            Init(syntax);
            Parent = parent;
        }

        private void Init(FieldDeclarationSyntax syntax)
        {
            if (syntax.Declaration.Variables.Count > 1)
                throw new ArgumentException(
                    "FieldDeclarationSyntax with more than one variable is not supported here.", nameof(syntax));

            this.syntax = syntax;

            Modifiers = FromRoslyn.MemberModifiers(syntax.Modifiers);
            name = new Identifier(syntax.Declaration.Variables.Single().Identifier);
        }

        public FieldDefinition(MemberModifiers modifiers, TypeReference type, string name, Expression initializer = null)
        {
            Modifiers = modifiers;
            Type = type;
            Name = name;
            Initializer = initializer;
        }

        public FieldDefinition(TypeReference type, string name, Expression initializer = null)
            : this(None, type, name, initializer) { }

        FieldDeclarationSyntax ISyntaxWrapper<FieldDeclarationSyntax>.GetWrapped(ref bool? changed)
        {
            GetAndResetChanged(ref changed);

            var declarator = syntax?.Declaration.Variables.Single();

            bool? thisChanged = false;

            var newAttributes = attributes?.GetWrapped(ref thisChanged) ?? syntax?.AttributeLists ?? default;
            var newModifiers = Modifiers;
            var newType = type?.GetWrapped(ref thisChanged) ?? syntax.Declaration.Type;
            var newName = name.GetWrapped(ref thisChanged);
            var newInitializer = initializerSet ? initializer?.GetWrapped(ref thisChanged) : declarator?.Initializer?.Value;

            if (syntax == null || FromRoslyn.MemberModifiers(syntax.Modifiers) != newModifiers || thisChanged == true ||
                !IsAnnotated(syntax))
            {
                var equalsValueClause =
                    newInitializer == null ? null : RoslynSyntaxFactory.EqualsValueClause(newInitializer);

                var newSyntax = RoslynSyntaxFactory.FieldDeclaration(
                    newAttributes, newModifiers.GetWrapped(), RoslynSyntaxFactory.VariableDeclaration(
                        newType, RoslynSyntaxFactory.SingletonSeparatedList(
                            RoslynSyntaxFactory.VariableDeclarator(newName, null, equalsValueClause))));

                syntax = Annotate(newSyntax);

                SetChanged(ref changed);
            }

            return syntax;
        }

        private protected override MemberDeclarationSyntax GetWrappedMember(ref bool? changed) =>
            this.GetWrapped<FieldDeclarationSyntax>(ref changed);

        private protected override void SetSyntaxImpl(Roslyn::SyntaxNode newSyntax)
        {
            Init((FieldDeclarationSyntax)newSyntax);

            SetList(ref attributes, null);
            Set(ref type, null);
            initializerSet = false;
            Set(ref initializer, null);
        }

        private protected override SyntaxNode CloneImpl() => new FieldDefinition(Modifiers, Type, Name, Initializer);

        protected override void ReplaceExpressionsImpl<T>(Func<T, bool> filter, Func<T, Expression> projection) =>
            Initializer = Expression.ReplaceExpressions(Initializer, filter, projection);
    }
}