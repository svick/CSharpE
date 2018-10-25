using CSharpE.Transform.VisualStudio;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace CSharpE.Transform.Tests
{
    public class SyntaxTreeDiffTests
    {
        [Fact]
        public void Adjust()
        {
            string oldCode = "class C { C(){} }";
            string newCode = "class Foo { int x; }";

            var diff = new SyntaxTreeDiff(
                SyntaxFactory.ParseSyntaxTree(oldCode), SyntaxFactory.ParseSyntaxTree(newCode));

            // C -> Foo
            Assert.Null(diff.Adjust(6));
            Assert.Equal(6, diff.AdjustLoose(6));

            // { -> {
            Assert.Equal(10, diff.Adjust(8));

            // space -> space
            Assert.Equal(11, diff.Adjust(9));

            // C -> int
            Assert.Null(diff.Adjust(10));
            Assert.Equal(12, diff.AdjustLoose(10));

            // space -> space
            Assert.Equal(18, diff.Adjust(15));

            // } -> }
            Assert.Equal(19, diff.Adjust(16));
        }
    }
}
