namespace CSharpE.Syntax.Internals
{
    internal interface ISyntaxWrapper<out TSyntax>
    {
        TSyntax GetWrapped(ref bool changed);
    }

    internal static class SyntaxWrapperExtensions
    {
        public static TSyntax GetWrapped<TSyntax>(this ISyntaxWrapper<TSyntax> syntaxWrapper)
        {
            bool changed = true;

            return syntaxWrapper.GetWrapped(ref changed);
        }
    }
}