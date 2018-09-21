using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CSharpE.Syntax;
using CSharpE.Transform;
using static CSharpE.Syntax.SyntaxFactory;
using static CSharpE.Syntax.MemberModifiers;

namespace CSharpE.Extensions.Actor
{
    public class ActorTransformation : SimpleTransformation
    {
        protected override void Process(Project project)
        {
            foreach (var type in project.GetTypesWithAttribute<ActorAttribute>().OfType<TypeDefinition>())
            {
                var actorSemaphoreField = type.AddField(
                    ReadOnly, typeof(SemaphoreSlim), "_actor_semaphore", New(typeof(SemaphoreSlim), Literal(1)));

                Expression actorSemaphoreFieldExpression = This().MemberAccess(actorSemaphoreField);

                foreach (var method in type.PublicMethods)
                {
                    method.ReturnType = TypeReference(typeof(Task<>), method.ReturnType);
                    method.IsAsync = true;

                    method.Body.Statements = new Statement[]
                    {
                        Await(actorSemaphoreFieldExpression.Call("WaitAsync")),
                        TryFinally(method.Body.Statements, new Statement[] { actorSemaphoreFieldExpression.Call("Release") })
                    };
                }
            }
        }
    }
}