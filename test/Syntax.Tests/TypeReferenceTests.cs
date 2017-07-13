using System;
using System.Collections.Generic;
using Xunit;

namespace CSharpE.Syntax.Tests
{
    public class TypeReferenceTests
    {
        [Fact]
        public void SimpleTypeCtor()
        {
            var typeReference = new TypeReference(typeof(Array));
            
            Assert.Equal("System.Array", typeReference.FullName);
            Assert.Empty(typeReference.TypeParameters);
        }

        [Fact]
        public void ClosedGenericTypeCtor()
        {
            var typeReference = new TypeReference(typeof(List<int>));

            Assert.Equal("System.Collections.Generic.List`1", typeReference.FullName);
            Assert.Equal(1, typeReference.TypeParameters.Length);
            Assert.Equal("System.Int32", typeReference.TypeParameters[0].FullName);
        }

        [Fact]
        public void OpenGenericTypeCtor()
        {
            var typeReference = new TypeReference(typeof(List<>));

            Assert.Equal("System.Collections.Generic.List`1", typeReference.FullName);
            Assert.Empty(typeReference.TypeParameters);
        }
    }
}
