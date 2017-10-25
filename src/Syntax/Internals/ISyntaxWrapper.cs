namespace CSharpE.Syntax.Internals
{
    /// <remarks>
    /// Requires that any type that implements <c>ISyntaxWrapper&lt;TSyntax&gt;</c>
    /// also has a constructor that takes <see cref="TSyntax"/>.
    /// </remarks>
    internal interface ISyntaxWrapper<TSyntax>
    {
        TSyntax GetWrapped();
    }
}