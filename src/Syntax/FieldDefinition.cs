using System;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSharpE.Syntax
{
    public class FieldDefinition : MemberDefinition
    {
        private FieldDeclarationSyntax syntax;
        private SourceFile containingFile;

        private FieldModifiers modifiers;
        private TypeReference type;
        private string name;
        private Expression initializer;

        public FieldDefinition(FieldDeclarationSyntax syntax, SourceFile containingFile)
        {
            this.syntax = syntax;
            this.containingFile = containingFile;
        }

        public FieldDefinition(FieldModifiers modifiers, Type type, string name, Expression initializer)
            : this(modifiers, new TypeReference(type), name, initializer)
        {
        }

        public FieldDefinition(FieldModifiers modifiers, TypeReference type, string name, Expression initializer)
        {
            this.modifiers = modifiers;
            this.type = type;
            this.name = name;
            this.initializer = initializer;
        }
    }
}