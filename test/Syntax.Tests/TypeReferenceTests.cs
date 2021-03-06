using System;
using System.Collections.Generic;
using System.Linq;
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
            Assert.Empty(typeReference.TypeArguments);
        }

        [Fact]
        public void ClosedGenericTypeCtor()
        {
            var typeReference = new NamedTypeReference(typeof(List<int>));

            Assert.Equal("System.Collections.Generic.List<System.Int32>", typeReference.FullName);
            Assert.Equal(1, typeReference.TypeArguments.Count);
            Assert.Equal("System.Int32", typeReference.TypeArguments[0].ToString());
        }

        [Fact]
        public void OpenGenericTypeCtor()
        {
            var typeReference = new NamedTypeReference(typeof(List<>));

            Assert.Equal("System.Collections.Generic.List", typeReference.FullName);
            Assert.Empty(typeReference.TypeArguments);
        }

        [Fact]
        public void UnknownTypeFromSyntax()
        {
            var file = new SourceFile("C.cse", @"class C { Foo.Bar f; }");
            new Project(new [] { file }, new LibraryReference[0]);
            var field = file.GetTypes().OfType<TypeDefinition>().Single().Fields.Single();
            var type = (NamedTypeReference)field.Type;

            Assert.Equal("Bar", type.Name);

            Assert.Equal("Foo", type.Container.Name);
            Assert.Null(type.Container.Container);

            Assert.Null(type.Namespace);

            Assert.Equal("Foo.Bar", type.FullName);
        }
    }
}
