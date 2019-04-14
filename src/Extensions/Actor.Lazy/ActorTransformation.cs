﻿using System.Collections.Generic;
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
            Smart.ForEach(project.GetTypesWithAttribute<ActorAttribute>(), baseType =>
            {
                if (!(baseType is TypeDefinition type))
                    return;

                var actorSemaphoreField = type.AddField(ReadOnly, typeof(SemaphoreSlim), "_actor_semaphore", New(typeof(SemaphoreSlim), Literal(1)));

                Expression actorSemaphoreFieldExpression = This().MemberAccess(actorSemaphoreField);

                Smart.ForEach(type.PublicMethods, actorSemaphoreFieldExpression, (asf, method) =>
                {
                    method.ReturnType = NamedType(typeof(Task<>), method.ReturnType);
                    method.IsAsync = true;

                    method.Body.Statements = new Statement[]
                    {
                        Await(asf.Call("WaitAsync")),
                        TryFinally(method.Body.Statements, new Statement[] { asf.Call("Release") })
                    };
                });
            });
        }
    }
}