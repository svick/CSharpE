using System.Linq;
using Xunit;

namespace CSharpE.Syntax.Tests
{
    public class ArgumentTests
    {
        private static Argument Parse(string code)
        {
            string classCode = $@"class C {{ int i = F({code}); }}";

            var file = new SourceFile("C.cs", classCode);

            var expression = (InvocationExpression)file.GetClasses().Single().Fields.Single().Initializer;

            return expression.Arguments.Single();
        }

        [Fact]
        public void ParseSimpleArgument()
        {
            string code = "null";
            var argument = Parse(code);

            Assert.IsType<NullExpression>(argument.Expression);
            Assert.Null(argument.Name);

            Assert.Equal(code, argument.ToString());
        }

        [Fact]
        public void ParseNamedArgument()
        {
            string code = "arg: null";
            var argument = Parse(code);

            Assert.IsType<NullExpression>(argument.Expression);
            Assert.Equal("arg", argument.Name);

            Assert.Equal(code, argument.ToString());
        }

        [Fact]
        public void CreteSimpleArgument()
        {
            var argument = new Argument(new NullExpression());

            Assert.Equal("null", argument.ToString());
        }

        [Fact]
        public void CreteNamedArgument()
        {
            var argument = new Argument("arg", new NullExpression());

            Assert.Equal("arg: null", argument.ToString());
        }

        [Fact]
        public void AddNameToArgument()
        {
            var argument = Parse("null");

            argument.Name = "arg";

            Assert.Equal("arg: null", argument.ToString());
        }

        [Fact]
        public void RemoveNameFromArgument()
        {
            var argument = Parse("arg: null");

            argument.Name = null;

            Assert.Equal("null", argument.ToString());
        }
    }
}
