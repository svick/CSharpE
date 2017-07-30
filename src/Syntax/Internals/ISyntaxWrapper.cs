namespace CSharpE.Syntax.Internals
{
    internal interface ISyntaxWrapper<out T>
    {
        T GetSyntax();

        // should be more efficient that asking for Changed; also resets private changed flag
        T GetChangedSyntaxOrNull();
    }
}