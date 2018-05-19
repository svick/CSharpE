using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.UnitTests;
using Xunit;
using static CSharpE.Syntax.SyntaxFactory;
using static CSharpE.Syntax.MemberModifiers;

namespace CSharpE.Transform.Tests
{
    public class FileDiffTests
    {
        [Fact(Skip = "FileDiff is not implemented")]
        public void BranchedDiffTest()
        {
            string initialCode = @"class C
{
    public int M()
    {
    }
}";

            var initialTree = SyntaxFactory.ParseSyntaxTree(initialCode);

            var initialSourceFile = new Syntax.SourceFile("C.cs", initialTree);

            // transform edit:

            initialSourceFile.Types.Single().AddField(
                ReadOnly, typeof(SemaphoreSlim), "_actor_semaphore", New(typeof(SemaphoreSlim), Literal(1)));

            var initialTransformedTree = initialSourceFile.GetWrapped();

            // user edit:

            var editedTree = initialTree.WithInsertBefore("    }", "        return 42;\n");

            var editedSourceFile = new Syntax.SourceFile("C.cs", editedTree);

            // transform edit again:

            editedSourceFile.Types.Single().AddField(
                ReadOnly, typeof(SemaphoreSlim), "_actor_semaphore", New(typeof(SemaphoreSlim), Literal(1)));

            var editedTransformedTree = editedSourceFile.GetWrapped();

            FileDiff.Between(initialTransformedTree, editedTransformedTree);
        }
    }
}