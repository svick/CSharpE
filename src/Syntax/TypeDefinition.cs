using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using CSharpSyntaxFactory = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace CSharpE.Syntax
{
    public class TypeDefinition : MemberDefinition, ITypeContainer
    {
        private bool changed;
        private TypeDeclarationSyntax syntaxNode;
        private SourceFile containingFile;
        
        internal TypeDefinition(TypeDeclarationSyntax typeDeclarationSyntax, SourceFile containingFile)
        {
            syntaxNode = typeDeclarationSyntax;
            this.containingFile = containingFile;
        }

        private string name;
        public string Name
        {
            get
            {
                if (name == null)
                    name = syntaxNode.Identifier.ValueText;
                
                return name;
            }
            set
            {
                changed = true;
                name = value;
            }
        }

        private string ns;
        public string Namespace
        {
            get
            {
                if (ns == null)
                {
                    string ComputeNamespace(NamespaceDeclarationSyntax namespaceDeclaration)
                    {
                        var result = new StringBuilder(namespaceDeclaration.Name.ToString());

                        namespaceDeclaration = (NamespaceDeclarationSyntax)namespaceDeclaration.Parent;

                        while (namespaceDeclaration != null)
                        {
                            result.Insert(0, '.');
                            result.Insert(0, namespaceDeclaration.ToString());
                        }

                        return result.ToString();
                    }

                    switch (syntaxNode.Parent)
                    {
                        case null:
                            throw new InvalidOperationException(); // TODO: ???
                        case CompilationUnitSyntax _:
                            ns = string.Empty;
                            break;
                        case NamespaceDeclarationSyntax nds:
                            ns = ComputeNamespace(nds);
                            break;
                        case TypeDeclarationSyntax _:
                            throw new NotImplementedException();
                        default:
                            throw new InvalidOperationException();
                    }
                }

                return ns;
            }
            set
            {
                changed = true;
                ns = value;
            }
        }

        public string FullName
        {
            get
            {
                if (Namespace == null)
                    return Name;

                return Namespace + "." + Name;
            }
        }

        private SyntaxListWrapper<MemberDefinition, MemberDeclarationSyntax> members;
        private SyntaxListWrapper<MemberDefinition, MemberDeclarationSyntax> MembersList
        {
            get
            {
                if (members == null)
                    members = syntaxNode.Members.Select(mds => MemberDefinition.Create(mds, this))
                        .ToWrapperList<MemberDefinition, MemberDeclarationSyntax>();

                return members;
            }
        }

        public IList<MemberDefinition> Members
        {
            get => MembersList;
            set => members = value.ToWrapperList<MemberDefinition, MemberDeclarationSyntax>();
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

        public bool HasAttribute(TypeReference attributeType) => HasAttribute(attributeType.FullName);
        
        private bool HasAttribute(string attributeTypeFullName)
        {
            var attributeLists = syntaxNode.AttributeLists;
            
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
            return field;
        }

        public FieldDefinition AddField(TypeReference type, string name, Expression initializer = null) =>
            AddField(MemberModifiers.None, type, name, initializer);

        public static implicit operator IdentifierExpression(TypeDefinition typeDefinition) => new IdentifierExpression(typeDefinition.Name);

        // TODO: modifiers stuff
        protected override void ValidateModifiers(MemberModifiers modifiers) => throw new System.NotImplementedException();

        private static ClassDeclarationSyntax CreateSyntax(
            string name, SyntaxList<MemberDeclarationSyntax> membersSyntax) =>
            CSharpSyntaxFactory.ClassDeclaration(name).WithMembers(membersSyntax);

        private static readonly Func<TypeDefinition, TypeDeclarationSyntax> SyntaxNodeGenerator = self =>
        {
            var membersSyntax = self.members == null
                ? self.syntaxNode.Members
                : CSharpSyntaxFactory.List(self.members.Select(m => m.GetSyntax()));

            return CreateSyntax(self.Name, membersSyntax);
        };

        public new TypeDeclarationSyntax GetSyntax() => wrapperHelper.GetSyntaxNode(ref syntaxNode, this, SyntaxNodeGenerator);

        public new TypeDeclarationSyntax GetChangedSyntaxOrNull()
        {
            var membersSyntax = members?.GetChangedSyntaxOrNull();

            if (membersSyntax == null && !wrapperHelper.Changed)
                return syntaxNode;
            
            wrapperHelper.ResetChanged();

            return CreateSyntax(Name, membersSyntax ?? syntaxNode.Members);
        }

        protected override MemberDeclarationSyntax GetSyntaxImpl() => GetSyntax();
        
        protected override MemberDeclarationSyntax GetChangedSyntaxOrNullImpl() => GetChangedSyntaxOrNull();
    }
}