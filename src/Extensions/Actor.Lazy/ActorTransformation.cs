using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CSharpE.Syntax;
using CSharpE.Syntax.Smart;
using CSharpE.Transform;
using static CSharpE.Syntax.SyntaxFactory;
using static CSharpE.Syntax.MemberModifiers;

namespace CSharpE.Extensions.Actor
{
    public class ActorTransformation : ITransformation
    {
        public void Process(Syntax.Project project)
        {
            project.ForEachTypeWithAttribute<ActorAttribute>(type =>
            {
                var actorSemaphoreField = type.AddField(ReadOnly, typeof(SemaphoreSlim), "_actor_semaphore", New(typeof(SemaphoreSlim), Literal(1)));

                type.ForEachPublicMethod(actorSemaphoreField.GetReference(), (asf, method) =>
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

        public IEnumerable<LibraryReference> AdditionalReferences =>
            new[] { new AssemblyReference(typeof(ActorAttribute)) };
    }
}