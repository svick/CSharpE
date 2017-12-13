using System;
using System.IO;
using System.Linq;
using System.Reflection;
using CSharpE.Extensions.Actor;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
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
            
            Assert.Equal(expectedOutput, ProcessSingleFile(input, transformation, typeof(ActorAttribute)));
        }

        [Fact]
        public void SimpleRoslynActor()
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

    readonly SemaphoreSlim _actor_semaphore = new SemaphoreSlim(1);
}";

            var transformation = new RoslynActorTransformation();

            Assert.Equal(expectedOutput, ProcessSingleFileWithRoslyn(input, transformation));
        }

        string ProcessSingleFileWithRoslyn(string code, RoslynActorTransformation transformation)
        {
            string referenceAssemblyPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                ".nuget/packages/microsoft.netcore.app/2.0.0/ref/netcoreapp2.0");

            var compilation = CSharpCompilation.Create(null)
                .AddSyntaxTrees(SyntaxFactory.ParseSyntaxTree(code))
                .AddReferences(
                    new[] { "mscorlib", "System.Runtime" }
                        .Select(a => MetadataReference.CreateFromFile(Path.Combine(referenceAssemblyPath, a + ".dll"))))
                .AddReferences(
                    MetadataReference.CreateFromFile(typeof(ActorAttribute).GetTypeInfo().Assembly.Location));

            return transformation.Process(compilation).SyntaxTrees.Single().ToString();
        }
    }
}
