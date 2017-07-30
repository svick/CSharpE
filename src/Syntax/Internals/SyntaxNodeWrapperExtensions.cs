using Microsoft.CodeAnalysis.CSharp;

namespace CSharpE.Syntax.Internals
{
    static class SyntaxNodeWrapperExtensions
    {
        public static string GetString(this ISyntaxWrapper<CSharpSyntaxNode> syntaxNodeWrapper) =>
            syntaxNodeWrapper.GetSyntax().ToFullString();
    }
}