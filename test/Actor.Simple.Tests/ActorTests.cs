using CSharpE.Extensions.Actor;
using CSharpE.TestUtilities;
using Xunit;
using static CSharpE.TestUtilities.TransformTestUtils;

namespace CSharpE.Actor.Simple.Tests
{
    public class ActorTests
    {
        [Fact]
        public void SimpleActor()
        {
            string input = @"using CSharpE.Extensions.Actor;

[Actor]
class C
{
    public int M()
    {
        return 42;
    }
}";

            string expectedOutput = @"using CSharpE.Extensions.Actor;
using System.Threading;
using System.Threading.Tasks;

[Actor]
class C
{
    public async Task<int> M()
    {
        await this._actor_semaphore.WaitAsync();
        try
        {
            return 42;
        }
        finally
        {
            this._actor_semaphore.Release();
        }
    }

    readonly SemaphoreSlim _actor_semaphore = new SemaphoreSlim(1);
}";

            var transformation = new ActorTransformation();
            
            AssertEx.LinesEqual(expectedOutput, ProcessSingleFile(input, transformation, typeof(ActorAttribute)));
        }
    }
}
