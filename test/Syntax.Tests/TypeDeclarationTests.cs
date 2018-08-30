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

        [Fact]
        public void StructsCanHaveInvalidModifiers()
        {

            string code = @"
abstract struct S0 {}
static struct S1 {}
sealed struct S2 {}";

            var file = new SourceFile("S.cs", code);

            var structDefinitions = file.Members.Select(ntd => ntd.GetTypeDefinition()).Cast<StructDefinition>().ToList();

            var struct0 = structDefinitions[0];

            Assert.Equal("S0", struct0.Name);
            Assert.Equal(MemberModifiers.Abstract, struct0.Modifiers);

            var struct1 = structDefinitions[1];

            Assert.Equal("S1", struct1.Name);
            Assert.Equal(MemberModifiers.Static, struct1.Modifiers);

            var struct2 = structDefinitions[2];

            Assert.Equal("S2", struct2.Name);
            Assert.Equal(MemberModifiers.Sealed, struct2.Modifiers);
        }
    }
}