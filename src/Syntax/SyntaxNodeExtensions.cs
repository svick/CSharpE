namespace CSharpE.Syntax
{
    public static class SyntaxNodeExtensions
    {
        public static T Clone<T>(this T node) where T : SyntaxNode => (T)node.Clone();
    }
}