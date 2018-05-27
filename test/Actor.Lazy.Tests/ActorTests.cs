using System.Linq;
using CSharpE.Extensions.Actor;
using CSharpE.TestUtilities;
using CSharpE.Transform;
using Microsoft.CodeAnalysis.CSharp.UnitTests;
using Xunit;
using static CSharpE.TestUtilities.TestUtils;

namespace CSharpE.Actor.Lazy.Tests
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
        public void SimpleActorProject()
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

            var sourceFile = new SourceFile("source.cse", input);

            var project = new Project(
                new[] { sourceFile }, new[] { typeof(ActorAttribute) },
                new ITransformation[] { new ActorTransformation() });

            var tranformedProject = project.Transform();
            Assert.Equal(expectedOutput, tranformedProject.SourceFiles.Single().Text);

        }

        [Fact]
        public void IncrementalActor()
        {
            string input = @"using CSharpE.Extensions.Actor;

class C
{
    public int M()
    {
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
        {[|
            return 42;|]
        }
        finally
        {
            this._actor_semaphore.Release();
        }
    }

    readonly SemaphoreSlim _actor_semaphore = new SemaphoreSlim(1);
}";

            var sourceFile = new SourceFile("source.cse", input);
            var project = new Project(
                new[] { sourceFile }, new[] { typeof(ActorAttribute) },
                new ITransformation[] { new ActorTransformation() });

            var recorder = new LogRecorder<LogAction>();
            project.Log += recorder.Record;

            var tranformedProject = project.Transform();
            Assert.Equal(input, tranformedProject.SourceFiles.Single().Text);
            Assert.Equal(
                recorder.Read(),
                new LogAction[] { ("TransformProject", null, "transform"), ("SourceFile", "source.cse", "transform") });

            sourceFile.Tree = sourceFile.Tree.WithInsertBefore("class", "[Actor]\n");
            tranformedProject = project.Transform();
            Assert.Equal(IgnoreOptional(expectedOutput), tranformedProject.SourceFiles.Single().Text);
            Assert.Equal(
                recorder.Read(),
                new LogAction[]
                {
                    ("TransformProject", null, "transform"), ("SourceFile", "source.cse", "transform"),
                    ("TypeDefinition", "C", "transform"), ("MethodDefinition", "M", "transform")
                });

            sourceFile.Tree = sourceFile.Tree.WithInsertBefore("    }", "        return 42;\n");
            tranformedProject = project.Transform();
            Assert.Equal(Includeptional(expectedOutput), tranformedProject.SourceFiles.Single().Text);
            Assert.Equal(
                recorder.Read(),
                new LogAction[]
                {
                    ("TransformProject", null, "transform"), ("SourceFile", "source.cse", "transform"),
                    ("TypeDefinition", "C", "transform"), ("MethodDefinition", "M", "transform")
                });
        }
    }
}
