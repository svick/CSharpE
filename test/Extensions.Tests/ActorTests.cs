using System;
using System.Linq;
using CSharpE.Extensions.Actor;
using CSharpE.Syntax;
using CSharpE.TestUtilities;
using CSharpE.Transform;
using CSharpE.Transform.Execution;
using Xunit;
using static CSharpE.TestUtilities.TransformTestUtils;

namespace CSharpE.Extensions.Tests
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

            var project = new ProjectTransformer(new ITransformation[] { new ActorTransformation() });

            var transformedProject = project.Transform(
                new Project(new[] { sourceFile }, CreateReferences(typeof(ActorAttribute))));
            AssertEx.LinesEqual(expectedOutput, transformedProject.SourceFiles.Single().GetText());
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
            var project = new Project(new[] { sourceFile }, CreateReferences(typeof(ActorAttribute)));
            var transformer = new ProjectTransformer(new ITransformation[] { new ActorTransformation() });

            var recorder = new LogRecorder<LogAction>();
            transformer.Log += recorder.Record;

            var transformedProject = transformer.Transform(project);
            AssertEx.LinesEqual(input, transformedProject.SourceFiles.Single().GetText());
            Assert.Equal(
                new LogAction[] { ("TransformProject", null, "transform"), ("SourceFile", "source.cse", "transform") },
                recorder.Read());

            string nl = Environment.NewLine;

            void SourceFileInsertBefore(string existingText, string newText)
            {
                var oldFile = project.SourceFiles[0];
                int offset = oldFile.GetText().IndexOf(existingText, StringComparison.Ordinal);
                var newFullText = oldFile.GetText().Insert(offset, newText);
                project.SourceFiles[0] = new SourceFile(oldFile.Path, newFullText);
            }

            SourceFileInsertBefore("class", $"[Actor]{nl}");
            transformedProject = transformer.Transform(project);
            AssertEx.LinesEqual(IgnoreOptional(expectedOutput), transformedProject.SourceFiles.Single().GetText());
            Assert.Equal(
                new LogAction[]
                {
                    ("TransformProject", null, "transform"), ("SourceFile", "source.cse", "transform"),
                    ("ClassDefinition", "C", "transform"), ("MethodDefinition", "M1", "transform"),
                    ("MethodDefinition", "M2", "transform")
                }, recorder.Read());

            SourceFileInsertBefore($"    }}{nl}}}", $"        return 42;{nl}");
            transformedProject = transformer.Transform(project);
            AssertEx.LinesEqual(IncludeOptional(expectedOutput), transformedProject.SourceFiles.Single().GetText());
            Assert.Equal(
                new LogAction[]
                {
                    ("TransformProject", null, "transform"), ("SourceFile", "source.cse", "transform"),
                    ("ClassDefinition", "C", "transform"), ("MethodDefinition", "M1", "cached"),
                    ("MethodDefinition", "M2", "transform")
                }, recorder.Read());

            transformedProject = transformer.Transform(project);
            AssertEx.LinesEqual(IncludeOptional(expectedOutput), transformedProject.SourceFiles.Single().GetText());
            Assert.Equal(
                new LogAction[] { ("TransformProject", null, "transform"), ("SourceFile", "source.cse", "cached") },
                recorder.Read());
        }
    }
}
