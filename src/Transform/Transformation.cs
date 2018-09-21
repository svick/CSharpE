using System;
using System.Collections.Generic;
using CSharpE.Syntax;
using static CSharpE.Syntax.SyntaxFactory;

namespace CSharpE.Transform
{
    public abstract class Transformation : ITransformation
    {
        public abstract void Process(Project project, bool designTime);

        public virtual IEnumerable<LibraryReference> AdditionalReferences => null;

        protected static Statement NotImplementedStatement { get; } = Throw(New(typeof(NotImplementedException)));
    }
}