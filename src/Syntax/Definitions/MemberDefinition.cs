using System;
using System.Collections.Generic;
using System.Linq;
using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static CSharpE.Syntax.MemberModifiers;

namespace CSharpE.Syntax
{
    public abstract class MemberDefinition : SyntaxNode, ISyntaxWrapper<MemberDeclarationSyntax>
    {
        internal MemberDefinition() { }

        internal MemberDefinition(MemberDeclarationSyntax memberDeclarationSyntax)
            : base(memberDeclarationSyntax) { }

        private protected abstract MemberDeclarationSyntax MemberSyntax { get; }

        private protected SyntaxList<Attribute, AttributeListSyntax> attributes;
        public IList<Attribute> Attributes
        {
            get
            {
                if (attributes == null)
                    attributes = new SyntaxList<Attribute, AttributeListSyntax>(GetAttributeLists(MemberSyntax), this);

                return attributes;
            }
            set => SetList(ref attributes, new SyntaxList<Attribute, AttributeListSyntax>(value, this));
        }

        public Attribute GetAttribute<T>() where T : System.Attribute => GetAttribute(typeof(T));

        public Attribute GetAttribute(NamedTypeReference attributeType) =>
            Attributes.SingleOrDefault(a => a.Type.Equals(attributeType));

        public bool HasAttribute<T>() where T : System.Attribute => HasAttribute(typeof(T));

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

        internal static SyntaxList<AttributeListSyntax> GetAttributeLists(MemberDeclarationSyntax syntax)
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

        private protected TypeDefinition ParentType => (TypeDefinition)Parent;

        MemberDeclarationSyntax ISyntaxWrapper<MemberDeclarationSyntax>.GetWrapped(ref bool? changed) =>
            GetWrappedMember(ref changed);

        private protected abstract MemberDeclarationSyntax GetWrappedMember(ref bool? changed);

        public void ReplaceExpressions<T>(Func<T, bool> filter, Func<T, Expression> projection) where T : Expression
        {
            foreach (var attribute in Attributes)
            {
                attribute.ReplaceExpressions(filter, projection);
            }

            ReplaceExpressionsImpl(filter, projection);
        }

        protected abstract void ReplaceExpressionsImpl<T>(Func<T, bool> filter, Func<T, Expression> projection)
            where T : Expression;
    }
}
