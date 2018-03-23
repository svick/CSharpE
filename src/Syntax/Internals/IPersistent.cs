namespace CSharpE.Syntax.Internals
{
    public interface IPersistent
    {
        /// <summary>
        /// Makes sure the current object has no references to objects that are not persisitent.
        /// </summary>
        void Persist();
    }
}