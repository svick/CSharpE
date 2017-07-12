﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CSharpE.Syntax;
using CSharpE.Transform;
using static CSharpE.Syntax.SyntaxFactory;

namespace CSharpE.Extensions.Actor
{
    public class ActorTransformation : ITransformation
    {
        public void Process(Project project)
        {
            var actorTypes = new List<TypeDefinition>();

            foreach (var type in project.TypesWithAttribute<ActorAttribute>())
            {
                actorTypes.Add(type);

                var actorDataField = type.Field(ReadOnly, typeof(SemaphoreSlim), "_actor_semaphore", New(typeof(SemaphoreSlim), Literal(1)));

                foreach (var method in type.PublicMethods)
                {
                    method.ReturnType = GenericTypeReference(typeof(Task<>), method.ReturnType);

                    method.Body = new Statement[]
                    {
                        Await(Call(actorDataField, "WaitAsync")),
                        TryFinally(method.Body, new Statement[] { Call(actorDataField, "WaitAsync") })
                    };
                }
            }
        }
    }
}