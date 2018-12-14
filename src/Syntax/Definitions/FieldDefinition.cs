using System;
using System.Linq;
using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static CSharpE.Syntax.MemberModifiers;
using RoslynSyntaxFactory = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Roslyn = Microsoft.CodeAnalysis;

// TODO: field declarations with more than one field
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

        private void Init(FieldDeclarationSyntax fieldDeclarationSyntax)
        {
            syntax = fieldDeclarationSyntax;

            // initialize non-lazy properties
            Modifiers = FromRoslyn.MemberModifiers(fieldDeclarationSyntax.Modifiers);
            name = new Identifier(fieldDeclarationSyntax.Declaration.Variables.First().Identifier);
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

            var declarator = syntax?.Declaration.Variables.First();

            bool? thisChanged = false;

            var newModifiers = Modifiers;
            var newType = type?.GetWrapped(ref thisChanged) ?? syntax.Declaration.Type;
            var newName = name.GetWrapped(ref thisChanged);
            var newInitializer = initializerSet ? initializer?.GetWrapped(ref thisChanged) : declarator?.Initializer?.Value;

            if (syntax == null || AttributesChanged() ||
                FromRoslyn.MemberModifiers(syntax.Modifiers) != newModifiers || thisChanged == true ||
                !IsAnnotated(syntax))
            {
                var equalsValueClause =
                    newInitializer == null ? null : RoslynSyntaxFactory.EqualsValueClause(newInitializer);

                var newSyntax = RoslynSyntaxFactory.FieldDeclaration(
                    GetNewAttributes(), newModifiers.GetWrapped(), RoslynSyntaxFactory.VariableDeclaration(
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
            ResetAttributes();

            Set(ref type, null);
            initializerSet = false;
            Set(ref initializer, null);
        }

        internal override SyntaxNode Clone() => new FieldDefinition(Modifiers, Type, Name, Initializer);
    }
}