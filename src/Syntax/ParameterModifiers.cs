using System;
using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using RoslynSyntaxFactory = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static CSharpE.Syntax.ParameterModifiers;
using static Microsoft.CodeAnalysis.CSharp.SyntaxKind;

namespace CSharpE.Syntax
{
    [Flags]
    public enum ParameterModifiers
    {
        None   = 0b00000,
        Ref    = 0b00001,
        Out    = 0b00010,
        In     = 0b00100,
        This   = 0b01000,
        Params = 0b10000
    }

    public static class ParameterModifiersExtensions
    {
        public static bool Contains(this ParameterModifiers modifiers, ParameterModifiers flag) => (modifiers & flag) != 0;

        public static ParameterModifiers With(this ParameterModifiers modifiers, ParameterModifiers flag, bool value = true) =>
            value ? modifiers | flag : modifiers & ~flag;

        internal static readonly BiDirectionalDictionary<ParameterModifiers, SyntaxKind> ModifiersMapping =
            new BiDirectionalDictionary<ParameterModifiers, SyntaxKind>
            {
                { Ref, RefKeyword },
                { Out, OutKeyword },
                { In, InKeyword },
                { This, ThisKeyword },
                { Params, ParamsKeyword }
            };

        internal static SyntaxTokenList GetWrapped(this ParameterModifiers modifiers)
        {
            var tokens = ModifiersMapping
                .Where((modifier, _) => modifiers.Contains(modifier))
                .Select((_, syntaxKind) => RoslynSyntaxFactory.Token(syntaxKind));

            return RoslynSyntaxFactory.TokenList(tokens);
        }
    }
}