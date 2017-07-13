using System;

namespace CSharpE.Syntax
{
    public class TypeReference
    {
        public string FullName { get; }
        
        public TypeReference(Type type)
        {
            // TODO: what about assembly?
            FullName = type.FullName;
        }
    }
}