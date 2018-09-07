using CSharpE.Extensions.Record;
using CSharpE.TestUtilities;
using Xunit;
using static CSharpE.TestUtilities.TransformTestUtils;

namespace Record.Tests
{
    public class RecordTests
    {
        [Fact]
        public void Test()
        {
            string input = @"using CSharpE.Extensions.Record;

[Record]
class Person
{
    string Name;
    System.Int32 Age;
}";

            string expectedOutput = @"using CSharpE.Extensions.Record;
using System;

[Record]
class Person : IEquatable<Person>
{
    public string Name
    {
        get;
        set;
    }

    public int Age
    {
        get;
        set;
    }
}";
            
            var transformation = new RecordTransformation();
            
            AssertEx.LinesEqual(expectedOutput, ProcessSingleFile(input, transformation, typeof(RecordAttribute)));
        }
    }
}