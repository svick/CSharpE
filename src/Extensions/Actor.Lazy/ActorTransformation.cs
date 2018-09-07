using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CSharpE.Syntax;
using CSharpE.Transform;
using CSharpE.Transform.Smart;
using static CSharpE.Syntax.SyntaxFactory;
using static CSharpE.Syntax.MemberModifiers;

namespace CSharpE.Extensions.Actor
{
    public class ActorTransformation : SimpleTransformation
    {
        protected override void Process(Syntax.Project project)
        {
            project.ForEachTypeWithAttribute<ActorAttribute>(baseType =>
            {
                if (!(baseType is TypeDefinition type))
                    return;

                var actorSemaphoreField = type.AddField(ReadOnly, typeof(SemaphoreSlim), "_actor_semaphore", New(typeof(SemaphoreSlim), Literal(1)));

                Expression actorSemaphoreFieldExpression = MemberAccess(This(), actorSemaphoreField);

                type.ForEachPublicMethod(actorSemaphoreFieldExpression, (asf, method) =>
                {
                    method.ReturnType = TypeReference(typeof(Task<>), method.ReturnType);
                    method.IsAsync = true;

                    method.Body = new Statement[]
                    {
                        Await(Call(asf, "WaitAsync")),
                        TryFinally(method.Body, new Statement[] { Call(asf, "Release") })
                    };
                });
            });
        }

        public override IEnumerable<LibraryReference> AdditionalReferences =>
            new[] { new AssemblyReference(typeof(ActorAttribute)) };
    }
}