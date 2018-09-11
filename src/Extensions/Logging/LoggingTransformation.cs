using System;
using System.Collections.Generic;
using CSharpE.Syntax;
using CSharpE.Transform;
using CSharpE.Transform.Smart;

namespace CSharpE.Extensions.Logging
{
    public class LoggingTransformation : BuildTimeTransformation
    {
        public override IEnumerable<LibraryReference> AdditionalReferences { get; }
        
        protected override void Process(Syntax.Project project)
        {
            project.ForEachMethod(method =>
            {
                
            });            
        }
    }
}
