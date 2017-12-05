using CSharpE.Extensions.Actor;
using Xunit;
using static TestUtilities.TestUtils;

namespace Actor.Tests
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
    SemaphoreSlim _actor_semaphore = new SemaphoreSlim(1);

    public async Task M()
    {
        await _actor_semaphore.WaitAsync();
        try
        {
            return 42;
        }
        finally
        {
            _actor_semaphore.Release();
        }
    }
}";

            var transformation = new ActorTransformation();
            
            Assert.Equal(expectedOutput, ProcessSingleFile(input, transformation, typeof(ActorAttribute)));
        }
    }
}
