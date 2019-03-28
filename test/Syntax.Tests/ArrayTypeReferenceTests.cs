using Xunit;

namespace CSharpE.Syntax.Tests
{
    public class ArrayTypeReferenceTests
    {
        [Fact]
        public void SimpleTest()
        {
            var arrayType = new ArrayTypeReference(new NamedTypeReference(typeof(int)));

            string code = arrayType.ToString();

            Assert.Equal("int[]", code);
        }

        [Fact]
        public void ArrayOfArrayTest()
        {
            var elementType = new ArrayTypeReference(new NamedTypeReference(typeof(int)));
            var arrayType = new ArrayTypeReference(elementType, 2);

            string code = arrayType.ToString();

            // The space is caused by https://github.com/dotnet/roslyn/issues/34565.
            Assert.Equal("int[, ][]", code);
        }
    }
}
