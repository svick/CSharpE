using System;
using System.Collections.Generic;
using System.Linq;
using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static CSharpE.Syntax.MemberModifiers;
using CSharpSyntaxFactory = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace CSharpE.Syntax
{
    public sealed class FieldDefinition : MemberDefinition, ISyntaxWrapper2<FieldDeclarationSyntax>
    {
        private const MemberModifiers ValidFieldModifiers =
            AccessModifiersMask | New | Static | Unsafe | Const | ReadOnly | Volatile;

        protected override void ValidateModifiers(MemberModifiers value)
        {
            var invalidModifiers = value & ~ValidFieldModifiers;
            if (invalidModifiers != 0)
                throw new ArgumentException(nameof(value), $"The modifiers {invalidModifiers} are not valid for a field.");
        }

        private FieldDeclarationSyntax syntax;

        protected override SyntaxList<AttributeListSyntax> GetSyntaxAttributes() => syntax?.AttributeLists ?? default;

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

        // null is a valid value for initializer, so another variable has to be used to track if it has already been lazily computed
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
                    {
                        initializer = FromRoslyn.Expression(syntaxInitializer.Value);
                    }

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
            this.syntax = syntax;
            Parent = parent;

            if (syntax.Declaration.Variables.Count > 1)
                throw new NotImplementedException("Field declarations with more than one field are not currently supported.");

            // initialize non-lazy properties
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

        public TypeDefinition ParentType { get; private set; }

        public static implicit operator MemberAccessExpression(FieldDefinition fieldDefinition) =>
            new MemberAccessExpression(fieldDefinition);

        internal FieldDeclarationSyntax GetWrapped(ref bool changed)
        {
            var declarator = syntax?.Declaration.Variables.Single();

            bool localChanged = false;

            var newModifiers = Modifiers;
            var newType = type?.GetWrapped(ref localChanged) ?? syntax.Declaration.Type;
            var newName = name.GetWrapped(ref localChanged);
            var newInitializer = initializerSet ? initializer?.GetWrapped() : declarator?.Initializer?.Value;

            if (syntax == null || AttributesChanged() ||
                FromRoslyn.MemberModifiers(syntax.Modifiers) != newModifiers || localChanged ||
                declarator.Initializer?.Value != newInitializer)
            {
                var equalsValueClause =
                    newInitializer == null ? null : CSharpSyntaxFactory.EqualsValueClause(newInitializer);

                syntax = CSharpSyntaxFactory.FieldDeclaration(
                    GetNewAttributes(), newModifiers.GetWrapped(), CSharpSyntaxFactory.VariableDeclaration(
                        newType, CSharpSyntaxFactory.SingletonSeparatedList(
                            CSharpSyntaxFactory.VariableDeclarator(newName, null, equalsValueClause))));

                changed = true;
            }

            return syntax;
        }

        protected override MemberDeclarationSyntax GetWrappedImpl() => GetWrapped();

        FieldDeclarationSyntax ISyntaxWrapper2<FieldDeclarationSyntax>.GetWrapped(ref bool changed) => GetWrapped(ref changed);

        protected override IEnumerable<IEnumerable<SyntaxNode>> GetChildren()
        {
            yield return Attributes;
            yield return Node(Type);
            if (Initializer != null)
                yield return Node(Initializer);
        }

        internal override SyntaxNode Parent
        {
            get => ParentType;
            set
            {
                if (value is TypeDefinition parentType)
                    ParentType = parentType;
                else
                    throw new ArgumentException(nameof(value));
            }
        }
    }
}