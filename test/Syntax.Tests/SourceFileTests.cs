using System.Linq;
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
    }
}