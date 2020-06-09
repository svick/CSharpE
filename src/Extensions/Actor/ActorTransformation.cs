using System.Threading;
using System.Threading.Tasks;
using CSharpE.Syntax;
using CSharpE.Transform;
using static CSharpE.Syntax.SyntaxFactory;
using static CSharpE.Syntax.MemberModifiers;

namespace CSharpE.Extensions.Actor
{
    public class ActorTransformation : Transformation
    {
        public override void Process(Project project, bool designTime)
        {
            Smart.ForEach(project.GetClassesWithAttribute<ActorAttribute>(), designTime, (designTime1, type) =>
            {
                var actorSemaphoreField = type.AddField(
                    ReadOnly, typeof(SemaphoreSlim), "_actor_semaphore", New(typeof(SemaphoreSlim), Literal(1)));

                Expression actorSemaphoreFieldExpression = This().MemberAccess(actorSemaphoreField);

                Smart.ForEach(type.PublicMethods, actorSemaphoreFieldExpression, designTime1, (asf, designTime2, method) =>
                {
                    if (method.IsStatic)
                        return;

                    bool needsYield = false;

                    if (!method.IsAsync)
                    {
                        method.ReturnType = method.ReturnType.Equals(NamedType(typeof(void)))
                            ? NamedType(typeof(Task))
                            : NamedType(typeof(Task<>), method.ReturnType);
                        method.IsAsync = true;
                        needsYield = true;
                    }

                    if (designTime2)
                    {
                        if (needsYield)
                            method.StatementBody.Statements.Insert(0, Await(NamedType(typeof(Task)).Call(nameof(Task.Yield))));
                    }
                    else
                    {
                        method.StatementBody.Statements = new Statement[]
                        {
                            Await(asf.Call("WaitAsync")),
                            TryFinally(method.StatementBody.Statements, new Statement[] { asf.Call("Release") })
                        };
                    }
                });
            });
        }
    }
}