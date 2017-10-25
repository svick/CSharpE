using System;
using System.Linq;
using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static CSharpE.Syntax.MemberModifiers;
using CSharpSyntaxFactory = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace CSharpE.Syntax
{
    public class FieldDefinition : MemberDefinition
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

        private SyntaxContext Context => ContainingType.Context;

        private TypeReference type;
        public TypeReference Type
        {
            get
            {
                if (type == null)
                    type = new TypeReference(syntax.Declaration.Type, Context);

                return type;
            }
            set => type = value ?? throw new ArgumentNullException();
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
                initializer = value;
                initializerSet = true;
            }
        }

        internal FieldDefinition(FieldDeclarationSyntax syntax, TypeDefinition containingType)
        {
            this.syntax = syntax;
            ContainingType = containingType;

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
        
        public static implicit operator MemberAccessExpression(FieldDefinition fieldDefinition) =>
            new MemberAccessExpression(fieldDefinition);

        internal FieldDeclarationSyntax GetWrapped()
        {
            var declarator = syntax.Declaration.Variables.Single();

            var newModifiers = Modifiers;
            var newType = type?.GetWrapped() ?? syntax.Declaration.Type;
            var newName = name.GetWrapped();
            var newInitializer = initializerSet ? initializer?.GetWrapped() : declarator.Initializer?.Value;

            if (syntax == null ||
                FromRoslyn.MemberModifiers(syntax.Modifiers) != newModifiers || syntax.Declaration.Type != newType ||
                declarator.Identifier != newName || declarator.Initializer?.Value != newInitializer)
            {
                var equalsValueClause =
                    newInitializer == null ? null : CSharpSyntaxFactory.EqualsValueClause(newInitializer);

                syntax = CSharpSyntaxFactory.FieldDeclaration(
                    default, newModifiers.GetWrapped(), CSharpSyntaxFactory.VariableDeclaration(
                        newType, CSharpSyntaxFactory.SingletonSeparatedList(
                            CSharpSyntaxFactory.VariableDeclarator(newName, null, equalsValueClause))));
            }

            return syntax;
        }

        protected override MemberDeclarationSyntax GetWrappedImpl() => GetWrapped();
    }
}