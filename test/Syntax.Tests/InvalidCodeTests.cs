using System.Linq;
using CSharpE.TestUtilities;
using Xunit;

namespace CSharpE.Syntax.Tests
{
    public class InvalidCodeTests
    {
        [Fact]
        public void InvalidClass()
        {
            string code = @"class C : { }";

            var file = SyntaxTestUtils.CreateSourceFile(code);

            var type = file.Members.Single().GetTypeDefinition();

            Assert.IsType<InvalidMemberDefinition>(type);
        }
    }
}