using CSharpE.Extensions.Record;
using CSharpE.TestUtilities;
using Xunit;
using static CSharpE.TestUtilities.TransformTestUtils;

namespace CSharpE.Record.Tests
{
    public class RecordTests
    {
        string input = @"using CSharpE.Extensions.Record;

[Record]
class Person
{
    string Name;
    System.Int32 Age;
}";
        
        [Fact]
        public void TestDesignTime()
        {
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

    public bool Equals(Person other)
    {
        throw new NotImplementedException();
    }

    public override bool Equals(object obj)
    {
        throw new NotImplementedException();
    }

    public override int GetHashCode()
    {
        throw new NotImplementedException();
    }
}";
            
            var transformation = new RecordTransformation();

            AssertEx.LinesEqual(
                expectedOutput, ProcessSingleFile(input, transformation, designTime: true, typeof(RecordAttribute)));
        }
        
        [Fact]
        public void TestBuildTime()
        {
            string expectedOutput = @"using CSharpE.Extensions.Record;
using System;
using System.Collections.Generic;

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

    public bool Equals(Person other)
    {
        if (object.ReferenceEquals(other, null))
        {
            return false;
        }

        if (object.ReferenceEquals(other, this))
        {
            return true;
        }

        return EqualityComparer<string>.Default.Equals(this.Name, other.Name) && EqualityComparer<int>.Default.Equals(this.Age, other.Age);
    }

    public override bool Equals(object obj)
    {
        throw new NotImplementedException();
    }

    public override int GetHashCode()
    {
        throw new NotImplementedException();
    }
}";
            
            var transformation = new RecordTransformation();

            Assert.Equal(
                expectedOutput, ProcessSingleFile(input, transformation, designTime: false, typeof(RecordAttribute)));
        }
        
    }
}