﻿using System.Threading;
using System.Threading.Tasks;
using CSharpE.Syntax;
using CSharpE.Transform;
using static CSharpE.Syntax.SyntaxFactory;
using static CSharpE.Syntax.MemberModifiers;

namespace CSharpE.Extensions.Actor
{
    public class ActorTransformation : ITransformation
    {
        public void Process(Project project)
        {
            foreach (var type in project.TypesWithAttribute<ActorAttribute>())
            {
                var actorSemaphoreField = type.AddField(ReadOnly, typeof(SemaphoreSlim), "_actor_semaphore", New(typeof(SemaphoreSlim), Literal(1)));

                foreach (var method in type.PublicMethods)
                {
                    method.ReturnType = TypeReference(typeof(Task<>), method.ReturnType);
                    method.IsAsync = true;

                    method.Body = new Statement[]
                    {
                        Await(Call(actorSemaphoreField, "WaitAsync")),
                        TryFinally(method.Body, new Statement[] { Call(actorSemaphoreField, "Release") })
                    };
                }
            }
        }
    }
}