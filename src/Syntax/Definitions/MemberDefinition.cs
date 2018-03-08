using System.Collections.Generic;
using System.Linq;
using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using CSharpSyntaxFactory = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace CSharpE.Syntax
{
    public abstract class MemberDefinition : ISyntaxWrapper<MemberDeclarationSyntax>
    {
        private List<Attribute> attributes;
        public IList<Attribute> Attributes
        {
            get
            {
                if (attributes == null)
                {
                    attributes = (from attributeList in GetSyntaxAttributes()
                        from attribute in attributeList.Attributes
                        select new Attribute(attribute)).ToList();
                }

                return attributes;
            }
            set => attributes = value.ToList();
        }

        protected abstract SyntaxList<AttributeListSyntax> GetSyntaxAttributes();

        protected bool AttributesChanged()
        {
            if (attributes == null)
                return false;

            var newAttributes = attributes.Select(a => a.GetWrapped(null));
            var oldAttributes = GetSyntaxAttributes().SelectMany(al => al.Attributes);

            return !newAttributes.SequenceEqual(oldAttributes);
        }

        protected SyntaxList<AttributeListSyntax> GetNewAttributes(WrapperContext context)
        {
            if (attributes == null)
                return GetSyntaxAttributes();

            return CSharpSyntaxFactory.List(
                attributes.Select(
                    a => CSharpSyntaxFactory.AttributeList(
                        CSharpSyntaxFactory.SingletonSeparatedList(a.GetWrapped(context)))));
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

        protected abstract void ValidateModifiers(MemberModifiers modifiers);

        public TypeDefinition ContainingType { get; internal set; }

        MemberDeclarationSyntax ISyntaxWrapper<MemberDeclarationSyntax>.GetWrapped(WrapperContext context) =>
            GetWrapped(context);

        internal MemberDeclarationSyntax GetWrapped(WrapperContext context) => GetWrappedImpl(context);

        protected abstract MemberDeclarationSyntax GetWrappedImpl(WrapperContext context);
    }
}