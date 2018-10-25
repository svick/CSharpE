using System;
using System.Collections.Generic;
using System.Linq;
using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static CSharpE.Syntax.MemberModifiers;
using CSharpSyntaxFactory = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Roslyn = Microsoft.CodeAnalysis;

namespace CSharpE.Syntax
{
    public abstract class MemberDefinition : SyntaxNode, ISyntaxWrapper<MemberDeclarationSyntax>
    {
        internal MemberDefinition() { }

        internal MemberDefinition(MemberDeclarationSyntax memberDeclarationSyntax)
            : base(memberDeclarationSyntax) { }

        private protected abstract MemberDeclarationSyntax MemberSyntax { get; }

        private List<Attribute> attributes;
        public IList<Attribute> Attributes
        {
            get
            {
                if (attributes == null)
                {
                    attributes = (from attributeList in GetAttributeLists(MemberSyntax)
                        from attribute in attributeList.Attributes
                        select new Attribute(attribute, this)).ToList();
                }

                return attributes;
            }
            set => attributes = value.ToList();
        }

        private protected bool AttributesChanged()
        {
            if (attributes == null)
                return false;

            var newAttributes = attributes.Select(a => a.GetWrapped());
            var oldAttributes = GetAttributeLists(MemberSyntax).SelectMany(al => al.Attributes);

            return !newAttributes.SequenceEqual(oldAttributes);
        }

        private protected SyntaxList<AttributeListSyntax> GetNewAttributes()
        {
            if (attributes == null)
                return GetAttributeLists(MemberSyntax);

            return CSharpSyntaxFactory.List(
                attributes.Select(
                    a => CSharpSyntaxFactory.AttributeList(
                        CSharpSyntaxFactory.SingletonSeparatedList(a.GetWrapped()))));
        }

        private protected void ResetAttributes() => attributes = null;

        public bool HasAttribute<T>() => HasAttribute(typeof(T));

        public bool HasAttribute(NamedTypeReference attributeType) => HasAttribute(attributeType.FullName);

        private bool HasAttribute(string attributeTypeFullName)
        {
            if (!GetAttributeLists(this.GetWrapped()).Any())
                return false;

            var attributeLists = GetAttributeLists((MemberDeclarationSyntax)GetSourceFileNode());

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

        private static SyntaxList<AttributeListSyntax> GetAttributeLists(MemberDeclarationSyntax syntax)
        {
            switch (syntax)
            {
                case null:
                    return default;
                case BaseFieldDeclarationSyntax fieldDeclaration:
                    return fieldDeclaration.AttributeLists;
                case BasePropertyDeclarationSyntax propertyDeclaration:
                    return propertyDeclaration.AttributeLists;
                case BaseMethodDeclarationSyntax methodDeclaration:
                    return methodDeclaration.AttributeLists;
                case BaseTypeDeclarationSyntax typeDeclaration:
                    return typeDeclaration.AttributeLists;
                case DelegateDeclarationSyntax delegateDeclaration:
                    return delegateDeclaration.AttributeLists;
                case IncompleteMemberSyntax incompleteMember:
                    return incompleteMember.AttributeLists;
            }

            throw new NotImplementedException(syntax.GetType().Name);
        }

        private MemberModifiers modifiers;
        public MemberModifiers Modifiers
        {
            get => modifiers;
            set
            {
                ValidateModifiers(value);
                modifiers = value;
            }
        }

        private protected abstract void ValidateModifiers(MemberModifiers modifiers);

        public MemberModifiers Accessibility
        {
            get => Modifiers.Accessibility();
            set => Modifiers = Modifiers.WithAccessibilityModifier(value);
        }

        public bool IsPublic => Accessibility == Public;
        public bool IsProtected => Accessibility == Protected;
        public bool IsInternal => Accessibility == Internal;
        public bool IsPrivate => Accessibility == Private;
        public bool IsProtectedInternal => Accessibility == ProtectedInternal;
        public bool IsPrivateProtected => Accessibility == PrivateProtected;

        private TypeDefinition parent;

        internal override SyntaxNode Parent
        {
            get => parent;
            set
            {
                if (value is TypeDefinition parentType)
                    parent = parentType;
                else
                    throw new ArgumentException(nameof(value));
            }
        }
        private protected TypeDefinition ParentType => parent;

        MemberDeclarationSyntax ISyntaxWrapper<MemberDeclarationSyntax>.GetWrapped(ref bool? changed) =>
            GetWrappedMember(ref changed);

        private protected abstract MemberDeclarationSyntax GetWrappedMember(ref bool? changed);
    }
}
