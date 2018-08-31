using System.Linq;
using CSharpE.TestUtilities;
using Xunit;

namespace CSharpE.Syntax.Tests
{
    public class TypeDeclarationTests
    {
        // TODO: add asserts for constructor and finalizer when those work
        [Fact]
        public void InterfaceCanHaveInvalidMembers()
        {

            string code = @"
interface I
{
    // field
    int f;

    // static method
    static void M() {}

    // constructor
    I() {}

    // finalizer
    ~I() {}
}";

            var file = SyntaxTestUtils.CreateSourceFile(code);

            var interfaceDefinition = (InterfaceDefinition)file.Members.Single().Value;

            var field = Assert.IsType<FieldDefinition>(interfaceDefinition.Members[0]);

            Assert.Equal("f", field.Name);
            Assert.Equal("System.Int32", field.Type.ToString());

            var method = Assert.IsType<MethodDefinition>(interfaceDefinition.Members[1]);

            Assert.Equal("M", method.Name);
            Assert.Equal(MemberModifiers.Static, method.Modifiers);
            Assert.Equal("System.Void", method.ReturnType.ToString());
        }
    }
}