using System;
using System.Collections.Generic;
using System.Text;
using CSharpE.Syntax;
using CSharpE.Transform;
using static CSharpE.Syntax.SyntaxFactory;

namespace CSharpE.Extensions.Logging
{
    public class LoggingTransformation : BuildTimeTransformation
    {
        protected override void Process(Project project)
        {
            Smart.ForEach(
                project.GetMethods(), method =>
                {
                    if (method.Body != null)
                    {
                        Statement loggingStatement = NamedType(typeof(Console))
                            .Call(nameof(Console.WriteLine), BuildWriteLineParameters(method));

                        method.Body.Statements.Insert(0, loggingStatement);
                    }
                });
        }

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

                // the cast is necessary, so that if there is a single array parameter,
                // it is not interpreted as the whole params array for WriteLine
                args.Add(Cast(typeof(object), Identifier(parameter.Name)));
            }

            formatString.Append(')');

            args.Insert(0, Literal(formatString.ToString()));

            return args;
        }
    }
}
