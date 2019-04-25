using CSharpE.Extensions.Logging;
using CSharpE.TestUtilities;
using Xunit;
using static CSharpE.TestUtilities.TransformTestUtils;

namespace CSharpE.Extensions.Tests
{
    public class LoggingTests
    {
        [Fact]
        public void Test()
        {
            string input = @"class C
{
    void M(int i) {}
}";

            string expectedOutput = @"using System;

class C
{
    void M(int i)
    {
        Console.WriteLine(""M(i: {0})"", (object)i);
    }
}";

            var transformation = new LoggingTransformation();

            AssertEx.LinesEqual(expectedOutput, ProcessSingleFile(input, transformation, false));
        }
    }
}