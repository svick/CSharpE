using System.Collections.Generic;
using System.Linq;
using CSharpE.Core;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSharpE.Syntax
{
    public class TypeDefinition
    {
        private TypeDeclarationSyntax syntax;
        private SourceFile containingFile;
        
        internal TypeDefinition(TypeDeclarationSyntax typeDeclarationSyntax, SourceFile containingFile)
        {
            syntax = typeDeclarationSyntax;
            this.containingFile = containingFile;
        }

        private string name;
        public string Name
        {
            get
            {
                if (name == null)
                    name = syntax.Identifier.ValueText;
                
                return name;
            }
            set => name = value;
        }

        private List<MemberDefinition> members;
        private List<MemberDefinition> MembersList
        {
            get
            {
                if (members == null)
                    members = syntax.Members.Select(mds => MemberDefinition.Create(mds, this)).ToList();

                return members;
            }
        }

        public IList<MemberDefinition> Members
        {
            get => MembersList;
            set => members = value.ToList();
        }

        public IList<MethodDefinition> Methods
        {
            get => FilteredList.Create<MemberDefinition, MethodDefinition>(MembersList);
            set => FilteredList.Set(MembersList, value);
        }

        public IList<MethodDefinition> PublicMethods
        {
            get => FilteredList.Create(MembersList, (MethodDefinition method) => method.Modifiers.Accessibility() == MemberModifiers.Public);
            set => FilteredList.Set(MembersList, method => method.Modifiers.Accessibility() == MemberModifiers.Public, value);
        }

        public bool HasAttribute<T>() => HasAttribute(typeof(T));

        public bool HasAttribute(TypeReference attributeType) => HasAttribute(attributeType.FullName);
        
        private bool HasAttribute(string attributeTypeFullName)
        {
            var attributeLists = syntax.AttributeLists;
            
            if (!attributeLists.Any())
                return false;

            var semanticModel = containingFile.SemanticModel;

            var attributeType = semanticModel.Compilation.GetTypeByMetadataName(attributeTypeFullName);

            foreach (var attributeSyntax in attributeLists.SelectMany(al => al.Attributes))
            {
                var typeSymbol = semanticModel.GetTypeInfo(attributeSyntax).Type;

                if (attributeType.Equals(typeSymbol))
                    return true;
            }

            return false;
        }

        public FieldDefinition Field(MemberModifiers modifiers, TypeReference type, string name, Expression initializer)
        {
            var field = new FieldDefinition(modifiers, type, name, initializer);
            this.Members.Add(field);
            return field;
        }
        
        public static implicit operator IdentifierExpression(TypeDefinition typeDefinition) => new IdentifierExpression(typeDefinition.Name);
    }
}