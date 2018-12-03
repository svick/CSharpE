using System.Linq;
using CSharpE.TestUtilities;
using Xunit;

namespace CSharpE.Syntax.Tests
{
    public class TypeDeclarationTests
    {
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

            var ctor = Assert.IsType<ConstructorDefinition>(interfaceDefinition.Members[2]);

            Assert.False(ctor.IsStatic);
            Assert.Equal(MemberModifiers.None, ctor.Modifiers);

            var finalizer = Assert.IsType<FinalizerDefinition>(interfaceDefinition.Members[3]);

            Assert.Equal(MemberModifiers.None, finalizer.Modifiers);
        }
    }
}