using System;
using System.Collections.Generic;

namespace CSharpE.Syntax.Internals
{
    // TODO: remove ISyntaxWrapper and ISyntaxWrapperBase and rename ISyntaxWrapper2
    /// <remarks>
    /// Requires that any type that implements <c>ISyntaxWrapper&lt;TSyntax&gt;</c>
    /// also has a constructor that takes <see cref="TSyntax"/>.
    /// </remarks>
    internal interface ISyntaxWrapper<out TSyntax> : ISyntaxWrapperBase<TSyntax>
    {
        TSyntax GetWrapped();
    }

    internal interface ISyntaxWrapper2<out TSyntax> : ISyntaxWrapperBase<TSyntax>
    {
        TSyntax GetWrapped(ref bool changed);
    }

    internal interface ISyntaxWrapperBase<out TSyntax> { }

    internal static class SyntaxWrapperExtensions
    {
        public static TSyntax GetWrapped<TSyntax>(
            this ISyntaxWrapperBase<TSyntax> syntaxWrapper, TSyntax oldSyntax, ref bool changed)
        {
            switch (syntaxWrapper)
            {
                case ISyntaxWrapper2<TSyntax> syntaxWrapper2:
                    return syntaxWrapper2.GetWrapped(ref changed);
                case ISyntaxWrapper<TSyntax> syntaxWrapper1:
                    var wrapped = syntaxWrapper1.GetWrapped();
                    changed = changed || !EqualityComparer<TSyntax>.Default.Equals(oldSyntax, wrapped);
                    return wrapped;
            }
            throw new InvalidOperationException();
        }

        public static TSyntax GetWrapped<TSyntax>(this ISyntaxWrapperBase<TSyntax> syntaxWrapper)
        {
            bool changed = true;

            return syntaxWrapper.GetWrapped(default, ref changed);
        }

        public static TSyntax GetWrapped<TSyntax>(this ISyntaxWrapper2<TSyntax> syntaxWrapper)
        {
            bool changed = true;

            return syntaxWrapper.GetWrapped(ref changed);
        }
    }
}