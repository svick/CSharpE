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
            var typeReference = new NamedTypeReference(typeof(Array));
            
            Assert.Equal("System.Array", typeReference.FullName);
            Assert.Empty(typeReference.TypeParameters);
        }

        [Fact]
        public void ClosedGenericTypeCtor()
        {
            var typeReference = new NamedTypeReference(typeof(List<int>));

            Assert.Equal("System.Collections.Generic.List", typeReference.FullName);
            Assert.Equal(1, typeReference.TypeParameters.Count);
            Assert.Equal("System.Int32", typeReference.TypeParameters[0].ToString());
        }

        [Fact]
        public void OpenGenericTypeCtor()
        {
            var typeReference = new NamedTypeReference(typeof(List<>));

            Assert.Equal("System.Collections.Generic.List", typeReference.FullName);
            Assert.Empty(typeReference.TypeParameters);
        }
    }
}
