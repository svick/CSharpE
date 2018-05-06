using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSharpE.Syntax.Internals
{
    /// <remarks>
    /// Requires that any type that implements <c>ISyntaxWrapper&lt;TSyntax&gt;</c>
    /// also has a constructor that takes <see cref="TSyntax"/>.
    /// </remarks>
    internal interface ISyntaxWrapper<out TSyntax>
    {
        TSyntax GetWrapped();
    }

    internal interface ISyntax<TSyntax> : ISyntaxWrapper<TSyntax>
    {
        void Push(TSyntax syntax);
    }

    internal static class SyntaxExtensions
    {
        // TODO: better name
        public static TSyntax GetWrappedValue<TSyntax>(this ISyntax<TSyntax> syntax)
        {
            var result = syntax.GetWrapped();
            syntax.Push(result);
            return result;
        }
    }
}