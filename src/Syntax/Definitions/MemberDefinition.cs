using System;
using System.Collections.Generic;
using System.Linq;
using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using CSharpSyntaxFactory = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Roslyn = Microsoft.CodeAnalysis;

namespace CSharpE.Syntax
{
    public abstract class MemberDefinition : SyntaxNode, ISyntaxWrapper<MemberDeclarationSyntax>
    {
        private List<Attribute> attributes;
        public IList<Attribute> Attributes
        {
            get
            {
                if (attributes == null)
                {
                    attributes = (from attributeList in GetAttributeLists(Syntax)
                        from attribute in attributeList.Attributes
                        select new Attribute(attribute, this)).ToList();
                }

                return attributes;
            }
            set => attributes = value.ToList();
        }

        private protected abstract MemberDeclarationSyntax Syntax { get; }

        private protected bool AttributesChanged()
        {
            if (attributes == null)
                return false;

            var newAttributes = attributes.Select(a => a.GetWrapped());
            var oldAttributes = GetAttributeLists(Syntax).SelectMany(al => al.Attributes);

            return !newAttributes.SequenceEqual(oldAttributes);
        }

        private protected SyntaxList<AttributeListSyntax> GetNewAttributes()
        {
            if (attributes == null)
                return GetAttributeLists(Syntax);

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

        private static Roslyn::SyntaxList<AttributeListSyntax> GetAttributeLists(MemberDeclarationSyntax syntax)
        {
            switch (syntax)
            {
                case BaseTypeDeclarationSyntax typeSyntax:
                    return typeSyntax.AttributeLists;
            }

            throw new NotImplementedException();
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

        MemberDeclarationSyntax ISyntaxWrapper<MemberDeclarationSyntax>.GetWrapped(ref bool? changed) =>
            GetWrappedImpl(ref changed);

        private protected abstract MemberDeclarationSyntax GetWrappedImpl(ref bool? changed);
    }
}