using System.Collections.Generic;
using CSharpE.Syntax;

namespace CSharpE.Transform
{
    public interface ITransformation
    {
        void Process(Syntax.Project project);

        IEnumerable<LibraryReference> AdditionalReferences { get; }
    }
}