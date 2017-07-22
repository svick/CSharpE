using System;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSharpE.Syntax
{
    public class FieldDefinition : MemberDefinition
    {
        private FieldDeclarationSyntax syntax;

        protected override void ValidateModifiers(MemberModifiers modifiers) => throw new NotImplementedException();

        public TypeReference Type { get; set; }
        
        public string Name { get; set; }

        public Expression Initializer { get; set; }

        public TypeDefinition ContaingingType { get; internal set; }

        internal FieldDefinition(FieldDeclarationSyntax syntax, TypeDefinition containgingType)
        {
            this.syntax = syntax;
            ContaingingType = containgingType;
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
    }
}