using System;
using CSharpE.Syntax;
using static CSharpE.Syntax.SyntaxFactory;

namespace CSharpE.Transform
{
    public abstract class Transformation : ITransformation
    {
        public abstract void Process(Project project, bool designTime);

        protected static Statement NotImplementedStatement { get; } = Throw(New(typeof(NotImplementedException)));
    }
}