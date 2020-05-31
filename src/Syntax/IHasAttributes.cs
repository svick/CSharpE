using System.Collections.Generic;
using System.Linq;

namespace CSharpE.Syntax
{
    public interface IHasAttributes
    {
        public IList<Attribute> Attributes { get; set; }
    }

    public static class HasAttributesExtensions
    {
        public static Attribute GetAttribute<T>(this IHasAttributes node) where T : System.Attribute => node.GetAttribute(typeof(T));

        public static Attribute GetAttribute(this IHasAttributes node, NamedTypeReference attributeType) =>
            node.Attributes.SingleOrDefault(a => a.Type.Equals(attributeType));

        public static bool HasAttribute<T>(this IHasAttributes node) where T : System.Attribute => node.HasAttribute(typeof(T));

        public static bool HasAttribute(this IHasAttributes node, NamedTypeReference attributeType) =>
            node.GetAttribute(attributeType) != null;
    }
}
