using System.Linq;
using Xunit;

namespace CSharpE.Syntax.Tests
{
    public class ErrorTests
    {
        [Fact]
        public void InvalidEnumBase()
        {
            var code = "enum E : {}";

            var file = new SourceFile("C.cs", code);

            Assert.IsType<InvalidMemberDefinition>(file.GetTypes().Single());
        }
    }
}
