using System;
using System.Linq;
using CSharpE.Extensions.Actor;
using CSharpE.TestUtilities;
using CSharpE.Transform;
using CSharpE.Transform.Execution;
using Microsoft.CodeAnalysis.CSharp.UnitTests;
using Xunit;
using static CSharpE.TestUtilities.TransformTestUtils;

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
            
            AssertEx.LinesEqual(expectedOutput, ProcessSingleFile(input, transformation, typeof(ActorAttribute)));
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

            var project = new ProjectTransformer(
                new[] { sourceFile }, CreateReferences(typeof(ActorAttribute)),
                new ITransformation[] { new ActorTransformation() });

            var transformedProject = project.Transform();
            AssertEx.LinesEqual(expectedOutput, transformedProject.SourceFiles.Single().Text);
        }

        [Fact]
        public void IncrementalActor()
        {
            string input = @"using CSharpE.Extensions.Actor;

class C
{
    public int M1()
    {
        return 0;
    }

    public int M2()
    {
    }
}";

            string expectedOutput = @"using CSharpE.Extensions.Actor;
using System.Threading;
using System.Threading.Tasks;

[Actor]
class C
{
    public async Task<int> M1()
    {
        await this._actor_semaphore.WaitAsync();
        try
        {
            return 0;
        }
        finally
        {
            this._actor_semaphore.Release();
        }
    }

    public async Task<int> M2()
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
            var project = new ProjectTransformer(
                new[] { sourceFile }, CreateReferences(typeof(ActorAttribute)),
                new ITransformation[] { new ActorTransformation() });

            var recorder = new LogRecorder<LogAction>();
            project.Log += recorder.Record;

            var tranformedProject = project.Transform();
            AssertEx.LinesEqual(input, tranformedProject.SourceFiles.Single().Text);
            Assert.Equal(
                new LogAction[] { ("TransformProject", null, "transform"), ("SourceFile", "source.cse", "transform") },
                recorder.Read());

            string nl = Environment.NewLine;

            sourceFile.Tree = sourceFile.Tree.WithInsertBefore("class", $"[Actor]{nl}");
            tranformedProject = project.Transform();
            AssertEx.LinesEqual(IgnoreOptional(expectedOutput), tranformedProject.SourceFiles.Single().Text);
            Assert.Equal(
                new LogAction[]
                {
                    ("TransformProject", null, "transform"), ("SourceFile", "source.cse", "transform"),
                    ("ClassDefinition", "C", "transform"), ("MethodDefinition", "M1", "transform"),
                    ("MethodDefinition", "M2", "transform")
                }, recorder.Read());

            sourceFile.Tree = sourceFile.Tree.WithInsertBefore($"    }}{nl}}}", $"        return 42;{nl}");
            tranformedProject = project.Transform();
            AssertEx.LinesEqual(IncludeOptional(expectedOutput), tranformedProject.SourceFiles.Single().Text);
            Assert.Equal(
                new LogAction[]
                {
                    ("TransformProject", null, "transform"), ("SourceFile", "source.cse", "transform"),
                    ("ClassDefinition", "C", "transform"), ("MethodDefinition", "M1", "cached"),
                    ("MethodDefinition", "M2", "transform")
                }, recorder.Read());

            tranformedProject = project.Transform();
            AssertEx.LinesEqual(IncludeOptional(expectedOutput), tranformedProject.SourceFiles.Single().Text);
            Assert.Equal(
                new LogAction[] { ("TransformProject", null, "transform"), ("SourceFile", "source.cse", "cached") },
                recorder.Read());
        }
    }
}
