using System;
using static CSharpE.Syntax.MemberModifiers;

namespace CSharpE.Syntax
{
    [Flags]
    public enum MemberModifiers
    {
        None      = 0b0000_0000_0000_0000_0000,

        // Access modifiers
        Public    = 0b0000_0000_0000_0000_0001,
        Protected = 0b0000_0000_0000_0000_0010,
        Internal  = 0b0000_0000_0000_0000_0100,
        Private   = 0b0000_0000_0000_0000_1000,
        ProtectedInternal = Protected | Internal,
        AccessModifiersMask = Public | Protected | Internal | Private,

        // Common modifiers
        New       = 0b0000_0000_0000_0001_0000,
        Static    = 0b0000_0000_0000_0010_0000,
        Unsafe    = 0b0000_0000_0000_0100_0000,

        // Class, method, property and event modifiers
        Abstract  = 0b0000_0000_0000_1000_0000,
        Sealed    = 0b0000_0000_0001_0000_0000,

        // Method, property and event modifiers
        Virtual   = 0b0000_0000_0010_0000_0000,
        Override  = 0b0000_0000_0100_0000_0000,
        Extern    = 0b0000_0000_1000_0000_0000,
        
        // Field modifiers
        Const     = 0b0000_0001_0000_0000_0000,
        ReadOnly  = 0b0000_0010_0000_0000_0000,
        Volatile  = 0b0000_0100_0000_0000_0000,

        // Class and method modifiers
        Partial   = 0b0000_1000_0000_0000_0000,

        // Method modifiers
        Async     = 0b0001_0000_0000_0000_0000
    }

    public static class MemberModifiersExtensions
    {
        public static bool Contains(this MemberModifiers modifiers, MemberModifiers flag) => (modifiers & flag) != 0;

        public static MemberModifiers With(this MemberModifiers modifiers, MemberModifiers flag, bool value = true) =>
            value ? modifiers | flag : modifiers & ~flag;

        public static MemberModifiers Accessibility(this MemberModifiers modifiers) =>
            modifiers & AccessModifiersMask;

        public static MemberModifiers WithAccessibilityModifier(this MemberModifiers modifiers, MemberModifiers accessModifier)
        {
            if ((accessModifier & ~AccessModifiersMask) != 0)
                throw new ArgumentException(nameof(accessModifier));

            return (modifiers & ~AccessModifiersMask) | accessModifier;
        }
    }
}