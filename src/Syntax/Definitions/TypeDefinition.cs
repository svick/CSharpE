using System;
using System.Collections.Generic;
using System.Linq;
using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using CSharpSyntaxFactory = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Roslyn = Microsoft.CodeAnalysis;

namespace CSharpE.Syntax
{
    public sealed class TypeDefinition : MemberDefinition, ITypeContainer, ISyntaxWrapper<TypeDeclarationSyntax>
    {
        private TypeDeclarationSyntax syntax;
        
        internal TypeDefinition(TypeDeclarationSyntax typeDeclarationSyntax, SyntaxNode parent)
        {
            Init(typeDeclarationSyntax);

            Parent = parent;
        }

        private void Init(TypeDeclarationSyntax typeDeclarationSyntax)
        {
            syntax = typeDeclarationSyntax;
            name = new Identifier(syntax.Identifier);
        }

        protected override SyntaxList<AttributeListSyntax> GetSyntaxAttributes() => syntax?.AttributeLists ?? default;

        private Identifier name;
        public string Name
        {
            get => name.Text;
            set => name.Text = value;
        }

        private MemberList members;
        private MemberList MembersList
        {
            get
            {
                if (members == null)
                    members = new MemberList(syntax.Members, this);

                return members;
            }
        }

        public IList<MemberDefinition> Members
        {
            get => MembersList;
            set => SetList(ref members, new MemberList(value, this));
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
            if (!syntax.AttributeLists.Any())
                return false;

            var attributeLists = ((TypeDeclarationSyntax)GetSourceFileNode()).AttributeLists;
            
            var semanticModel = SourceFile.SemanticModel;

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

        public static implicit operator IdentifierExpression(TypeDefinition typeDefinition) =>
            new IdentifierExpression(typeDefinition.Name);

        // TODO: namespace
        public NamedTypeReference GetReference() => new NamedTypeReference(null, Name);

        protected override void ValidateModifiers(MemberModifiers modifiers) => throw new NotImplementedException();

        internal TypeDeclarationSyntax GetWrapped(ref bool? changed)
        {
            GetAndResetChanged(ref changed);

            bool? thisChanged = false;

            var newName = name.GetWrapped(ref thisChanged);
            var newMembers = members?.GetWrapped(ref thisChanged) ?? syntax.Members;

            if (syntax == null || AttributesChanged() || thisChanged == true || !IsAnnotated(syntax))
            {
                var newSyntax = CSharpSyntaxFactory.ClassDeclaration(
                    GetNewAttributes(), default, newName, default, default, default, newMembers);

                syntax = Annotate(newSyntax);

                SetChanged(ref changed);
            }

            return syntax;
        }

        protected override MemberDeclarationSyntax GetWrappedImpl(ref bool? changed) => GetWrapped(ref changed);

        TypeDeclarationSyntax ISyntaxWrapper<TypeDeclarationSyntax>.GetWrapped(ref bool? changed) =>
            GetWrapped(ref changed);

        private protected override void SetSyntaxImpl(Roslyn::SyntaxNode newSyntax)
        {
            Init((TypeDeclarationSyntax)newSyntax);
            ResetAttributes();

            members = null;
        }

        internal override SyntaxNode Clone()
        {
            throw new NotImplementedException();
        }

        internal override SyntaxNode Parent { get; set; }
    }
}