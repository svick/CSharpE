﻿using System;
using System.Collections.Generic;
using System.Linq;
using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using CSharpSyntaxFactory = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace CSharpE.Syntax
{
    public class TypeDefinition : MemberDefinition, ITypeContainer
    {
        private TypeDeclarationSyntax syntax;
        private SourceFile containingFile;
        
        internal TypeDefinition(TypeDeclarationSyntax typeDeclarationSyntax, SourceFile containingFile)
        {
            syntax = typeDeclarationSyntax;
            this.containingFile = containingFile;

            name = new Identifier(syntax.Identifier);
        }

        protected override SyntaxList<AttributeListSyntax> GetSyntaxAttributes() => syntax?.AttributeLists ?? default;

        private Identifier name;
        public string Name
        {
            get => name.Text;
            set => name.Text = value;
        }

        private SyntaxList<MemberDefinition, MemberDeclarationSyntax> members;
        private SyntaxList<MemberDefinition, MemberDeclarationSyntax> MembersList
        {
            get
            {
                if (members == null)
                    members = new SyntaxList<MemberDefinition, MemberDeclarationSyntax>(
                        syntax.Members, mds => FromRoslyn.MemberDefinition(mds, this));

                return members;
            }
        }

        public IList<MemberDefinition> Members
        {
            get => MembersList;
            set => members = new SyntaxList<MemberDefinition, MemberDeclarationSyntax>(value);
        }

        public IList<FieldDefinition> Fields
        {
            get => FilteredList.Create<MemberDefinition, FieldDefinition>(MembersList);
            set => FilteredList.Set(MembersList, value);
        }

        public IList<MethodDefinition> Methods
        {
            get => FilteredList.Create<MemberDefinition, MethodDefinition>(MembersList);
            set => FilteredList.Set(MembersList, value);
        }

        public IList<MethodDefinition> PublicMethods
        {
            get => FilteredList.Create(MembersList, (MethodDefinition method) => method.IsPublic);
            set => FilteredList.Set(MembersList, method => method.IsPublic, value);
        }

        public IList<TypeDefinition> Types
        {
            get => FilteredList.Create<MemberDefinition, TypeDefinition>(MembersList);
            set => FilteredList.Set(MembersList, value);
        }

        IEnumerable<TypeDefinition> ITypeContainer.Types => Types;

        public bool HasAttribute<T>() => HasAttribute(typeof(T));

        public bool HasAttribute(NamedTypeReference attributeType) => HasAttribute(attributeType.FullName);
        
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

        public FieldDefinition AddField(
            MemberModifiers modifiers, TypeReference type, string name, Expression initializer = null)
        {
            var field = new FieldDefinition(modifiers, type, name, initializer);
            this.Members.Add(field);

            // TODO: SyntaxList will probably have to notify its parent, so that it can set ContainingType of the child
            field.Parent = this;

            return field;
        }

        public FieldDefinition AddField(TypeReference type, string name, Expression initializer = null) =>
            AddField(MemberModifiers.None, type, name, initializer);

        public static implicit operator IdentifierExpression(TypeDefinition typeDefinition) => new IdentifierExpression(typeDefinition.Name);

        // TODO: namespace
        public NamedTypeReference GetReference() => new NamedTypeReference(null, Name);

        public Project Project => containingFile?.Project;

        protected override void ValidateModifiers(MemberModifiers modifiers) => throw new NotImplementedException();

        internal new TypeDeclarationSyntax GetWrapped()
        {
            var newName = name.GetWrapped();
            var newMembers = members?.GetWrapped() ?? syntax.Members;

            if (syntax == null || AttributesChanged() || newName != syntax.Identifier || newMembers != syntax.Members || IsAnnotated(syntax))
            {
                var newSyntax = CSharpSyntaxFactory.ClassDeclaration(
                    GetNewAttributes(), default, name.GetWrapped(), default, default, default,
                    newMembers);

                syntax = Annotate(newSyntax);
            }

            return syntax;
        }

        protected override MemberDeclarationSyntax GetWrappedImpl() => GetWrapped();

        protected override IEnumerable<IEnumerable<SyntaxNode>> GetChildren()
        {
            yield return Attributes;
            yield return Members;
        }
    }
}