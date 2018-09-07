using System;
using System.Collections.Generic;
using CSharpE.Syntax;
using static CSharpE.Syntax.SyntaxFactory;

namespace CSharpE.Transform
{
    public abstract class Transformation : ITransformation
    {
        public abstract void Process(Syntax.Project project, bool designTime);
        
        public abstract IEnumerable<LibraryReference> AdditionalReferences { get; }

        public static Statement NotImplementedStatement { get; } = Throw(New(typeof(NotImplementedException)));
    }
}