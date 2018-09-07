using System.Collections.Generic;
using CSharpE.Syntax;

namespace CSharpE.Transform
{
    public abstract class Transformation : ITransformation
    {
        public abstract void Process(Syntax.Project project, bool designTime);
        
        public abstract IEnumerable<LibraryReference> AdditionalReferences { get; }

        // TODO
        //public static Statement NotImplementedStatement { get; } = ...;
    }
}