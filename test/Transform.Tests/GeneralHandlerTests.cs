using System.Collections.Generic;
using CSharpE.Transform.Internals;
using Xunit;

namespace CSharpE.Transform.Tests
{
    public class GeneralHandlerTests
    {
        [Fact]
        public void ArrayCanBeCloned()
        {
            var source = new[] { 1, 2, 3 };

            var cloned = GeneralHandler.DeepClone(source);

            Assert.Equal(source, cloned);
        }

        [Fact]
        public void ListCanBeCloned()
        {
            var source = new List<int> { 1, 2, 3 };

            var cloned = GeneralHandler.DeepClone(source);

            Assert.Equal(source, cloned);
        }
    }
}