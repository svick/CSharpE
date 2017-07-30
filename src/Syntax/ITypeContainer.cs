using System.Collections.Generic;

namespace CSharpE.Syntax
{
    public interface ITypeContainer
    {
        IEnumerable<TypeDefinition> Types { get; }
    }
}