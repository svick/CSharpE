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

            Assert.IsType<InvalidMemberDefinition>(file.Members.Single().GetTypeDefinition());
        }

        [Fact]
        public void InvalidEnumBase()
        {
            var code = "enum E : {}";

            var file = SyntaxTestUtils.CreateSourceFile(code);

            Assert.IsType<InvalidMemberDefinition>(file.GetTypes().Single());
        }
    }
}