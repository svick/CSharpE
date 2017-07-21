using System;
using System.Collections.Generic;
using Xunit;
using static CSharpE.Syntax.MemberModifiers;

namespace CSharpE.Syntax.Tests
{
    public class MemberModifiersTests
    {
        [Fact]
        public void WithInvalidAccessModifierThrows()
        {
            Assert.Throws<ArgumentException>(() => None.WithAccessibilityModifier(Const));
        }

        public static IEnumerable<object[]> AccessModifiers()
        {
            yield return new object[] { Public };
            yield return new object[] { Protected };
            yield return new object[] { Internal };
            yield return new object[] { Private };
            yield return new object[] { ProtectedInternal };
        }

        [Theory, MemberData(nameof(AccessModifiers))]
        public void NoneWithAccessModifier(MemberModifiers accessModifier)
        {
            Assert.Equal(accessModifier, None.WithAccessibilityModifier(accessModifier));
        }

        [Fact]
        public void ComplicatedWithAccessModifier()
        {
            Assert.Equal(Public | Static | Async, (Private | Static | Async).WithAccessibilityModifier(Public));
        }
    }
}