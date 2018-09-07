using System.Collections.Generic;
using System.Linq;
using CSharpE.Syntax;
using CSharpE.TestUtilities;
using Xunit;
using static CSharpE.TestUtilities.TransformTestUtils;

namespace CSharpE.Transform.Tests
{
    public class AddFieldTests
    {
        class AddFieldTransformation : SimpleTransformation
        {
            protected override void Process(Syntax.Project project)
            {
                foreach (var type in project.GetTypes().OfType<TypeDefinition>())
                {
                    type.AddField(typeof(int), "i");
                }
            }

            public override IEnumerable<LibraryReference> AdditionalReferences => Enumerable.Empty<LibraryReference>();
        }

        [Fact]
        public void AddField()
        {
            string input = @"class C
{
}";

            string expectedOutput = @"class C
{
    int i;
}";

            var transformation = new AddFieldTransformation();

            AssertEx.LinesEqual(expectedOutput, ProcessSingleFile(input, transformation));
        }
    }
}
