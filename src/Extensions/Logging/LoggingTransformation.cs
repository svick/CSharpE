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
            foreach (var method in project.GetMethods())
            {
                if (method.Body != null)
                {
                    Statement loggingStatement = NamedType(typeof(Console))
                        .Call(nameof(Console.WriteLine), InterpolatedString(BuildInterpolation(method)));

                    method.Body.Statements.Insert(0, loggingStatement);
                }
            }
        }

        private static IEnumerable<InterpolatedStringContent> BuildInterpolation(MethodDefinition method)
        {
            yield return method.Name + "(";

            bool first = true;

            foreach (var parameter in method.Parameters)
            {
                if (!first)
                    yield return ", ";
                first = false;

                yield return parameter.Name + ": ";

                yield return Interpolation(Identifier(parameter.Name));
            }

            yield return ")";
        }
    }
}
