using System;
using System.Linq;

namespace CSharpE.Syntax
{
    public class TypeReference
    {
        public string FullName { get; set; }
        
        public TypeReference[] TypeParameters { get; set; }

        public TypeReference(string fullName, params TypeReference[] typeParameters)
        {
            FullName = fullName;
            TypeParameters = typeParameters;
        }

        public TypeReference(Type type)
        {
            // type.IsGenericType is not in .Net Standard 1.x
            if (!type.GenericTypeArguments.Any() || !type.IsConstructedGenericType)
            {
                // TODO: what about assembly?
                FullName = type.FullName;
                TypeParameters = Array.Empty<TypeReference>();
            }
            else
            {
                FullName = type.GetGenericTypeDefinition().FullName;
                TypeParameters = type.GenericTypeArguments.Select(a => new TypeReference(a)).ToArray();
            }
        }

        public TypeReference(TypeReference openGenericType, params TypeReference[] typeParameters)
        {
            FullName = openGenericType.FullName;
            TypeParameters = typeParameters.ToArray();
        }

        public static implicit operator TypeReference(Type type) => new TypeReference(type);
    }
}