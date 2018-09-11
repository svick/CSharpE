using System;
using System.Collections.Generic;
using System.Text;
using CSharpE.Syntax;
using CSharpE.Transform;
using CSharpE.Transform.Smart;
using static CSharpE.Syntax.SyntaxFactory;

namespace CSharpE.Extensions.Logging
{
    public class LoggingTransformation : BuildTimeTransformation
    {
        public override IEnumerable<LibraryReference> AdditionalReferences { get; }

        private static IEnumerable<Expression> BuildWriteLineParameters(MethodDefinition method)
        {
            var formatString = new StringBuilder();
            var args = new List<Expression>();

            formatString.Append(method.Name);

            formatString.Append('(');

            int i = 0;
            
            foreach (var parameter in method.Parameters)
            {
                if (i != 0)
                    formatString.Append(", ");
                
                formatString.Append(parameter.Name)
                    .Append(": {")
                    .Append(i++)
                    .Append('}');
                
                args.Add(Identifier(parameter.Name));
            }

            formatString.Append(')');
            
            args.Insert(0, Literal(formatString.ToString()));

            return args;
        }
        
        protected override void Process(Syntax.Project project)
        {
            project.ForEachMethod(method =>
            {
                Statement loggingStatement = TypeReference(typeof(Console))
                    .Call(nameof(Console.WriteLine), BuildWriteLineParameters(method));

                method.Body = Block(loggingStatement, method.Body);
            });            
        }
    }
}
