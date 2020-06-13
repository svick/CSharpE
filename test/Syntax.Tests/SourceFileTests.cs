using System.Linq;
using CSharpE.TestUtilities;
using Xunit;

namespace CSharpE.Syntax.Tests
{
    public class SourceFileTests
    {
        [Fact]
        public void GetAllTypesTest()
        {
            string code = @"
class A {}

class B
{
    class C
    {
        class D {}
    }

    class E {}
}";

            var sourceFile = new SourceFile("Types.cs", code);

            Assert.Equal(new[] { "A", "B", "C", "D", "E" }, sourceFile.GetAllTypes().Select(t => t.Name));
        }

        [Fact]
        public void SortUsingsTest()
        {
            string code = @"using A;
using static A.B;
using System.IO;
using X = A.C";

            var sourceFile = new SourceFile("C.cs", code);

            sourceFile.EnsureUsingNamespace("A.B.C");

            var expected = @"using System.IO;
using A;
using A.B.C;
using static A.B;
using X = A.C";

            AssertEx.LinesEqual(expected, sourceFile.GetText());
        }
    }
}