using System;
using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using static CSharpE.Syntax.MemberModifiers;
using static Microsoft.CodeAnalysis.CSharp.SyntaxKind;
using CSharpSyntaxFactory = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace CSharpE.Syntax
{
    [Flags]
    public enum MemberModifiers
    {
        None      = 0b0000_0000_0000_0000_0000,

        // Access modifiers
        Private   = 0b0000_0000_0000_0000_0001,
        Protected = 0b0000_0000_0000_0000_0010,
        Internal  = 0b0000_0000_0000_0000_0100,
        Public    = 0b0000_0000_0000_0000_1000,
        ProtectedInternal = Protected | Internal,
        PrivateProtected = Private | Protected,
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

        internal static readonly BiDirectionalDictionary<MemberModifiers, SyntaxKind> ModifiersMapping =
            new BiDirectionalDictionary<MemberModifiers, SyntaxKind>
            {
                { Private, PrivateKeyword },
                { Protected, ProtectedKeyword },
                { Internal, InternalKeyword },
                { Public, PublicKeyword },
                { New, NewKeyword },
                { Static, StaticKeyword },
                { Unsafe, UnsafeKeyword },
                { Abstract, AbstractKeyword },
                { Sealed, SealedKeyword },
                { Virtual, VirtualKeyword },
                { Override, OverrideKeyword },
                { Extern, ExternKeyword },
                { Const, ConstKeyword },
                { ReadOnly, ReadOnlyKeyword },
                { Volatile, VolatileKeyword },
                { Partial, PartialKeyword },
                { Async, AsyncKeyword }
            };

        internal static SyntaxTokenList GetWrapped(this MemberModifiers modifiers)
        {
            var tokens = ModifiersMapping
                .Where((modifier, _) => modifiers.Contains(modifier))
                .Select((_, syntaxKind) => CSharpSyntaxFactory.Token(syntaxKind));

            return CSharpSyntaxFactory.TokenList(tokens);
        }
    }
}