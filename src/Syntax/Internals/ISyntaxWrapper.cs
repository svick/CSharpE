using System.Diagnostics;

namespace CSharpE.Syntax.Internals
{
    internal interface ISyntaxWrapper<out TSyntax>
    {
        TSyntax GetWrapped(ref bool? changed);
    }

    internal static class SyntaxWrapperExtensions
    {
        public static TSyntax GetWrapped<TSyntax>(this ISyntaxWrapper<TSyntax> syntaxWrapper)
        {
            bool? changed = null;

            return syntaxWrapper.GetWrapped(ref changed);
        }

        [DebuggerHidden]
        public static TSyntax GetWrapped<TSyntax>(this ISyntaxWrapper<TSyntax> syntaxWrapper, ref bool? changed)
        {
            // this would very likely be a temporary boxed value type, so the original would not remember that is reported changed
            Debug.Assert(!syntaxWrapper.GetType().IsValueType);

            return syntaxWrapper.GetWrapped(ref changed);
        }
    }
}