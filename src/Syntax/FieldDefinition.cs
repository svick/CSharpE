using System;
using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static CSharpE.Syntax.MemberModifiers;

namespace CSharpE.Syntax
{
    public class FieldDefinition : MemberDefinition, ISyntaxWrapper<FieldDeclarationSyntax>
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

        public TypeReference Type { get; set; }
        
        public string Name { get; set; }

        public Expression Initializer { get; set; }

        internal FieldDefinition(FieldDeclarationSyntax syntax, TypeDefinition containingType)
        {
            this.syntax = syntax;
            ContainingType = containingType;
        }

        public FieldDefinition(MemberModifiers modifiers, TypeReference type, string name, Expression initializer)
        {
            Modifiers = modifiers;
            Type = type;
            Name = name;
            Initializer = initializer;
        }
        
        public static implicit operator MemberAccessExpression(FieldDefinition fieldDefinition) =>
            new MemberAccessExpression(fieldDefinition);

        public new FieldDeclarationSyntax GetSyntax() => throw new NotImplementedException();

        public new FieldDeclarationSyntax GetChangedSyntaxOrNull() => throw new NotImplementedException();

        protected override MemberDeclarationSyntax GetSyntaxImpl() => GetSyntax();

        protected override MemberDeclarationSyntax GetChangedSyntaxOrNullImpl() => GetChangedSyntaxOrNull();
    }
}