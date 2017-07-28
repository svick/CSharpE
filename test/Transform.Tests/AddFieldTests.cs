using CSharpE.Syntax;
using CSharpE.Transform;
using Xunit;
using static TestUtilities.TestUtils;

namespace Transform.Tests
{
    public class AddFieldTests
    {
        class AddFieldTransformation : ITransformation
        {
            public void Process(Project project)
            {
                foreach (var type in project.Types())
                {
                    type.AddField(typeof(int), "i");
                }
            }
        }

        [Fact]
        public void AddField()
        {
            string input = @"
class C
{
}
";

            string expectedOutput = @"
class C
{
    int i;
}
";

            var transformation = new AddFieldTransformation();

            Assert.Equal(ProcessSingleFile(input, transformation), expectedOutput);
        }
    }
}
