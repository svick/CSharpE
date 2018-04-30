using System;

namespace CSharpE.Transform.Internals
{
    internal static class DescendantFinder
    {
        public static Func<TAncestor, TDescendant> Create<TAncestor, TDescendant>(TAncestor ancestor, TDescendant descendant)
        {
            if (ReferenceEquals(ancestor, descendant))
                return a => (TDescendant)(object)a;

            throw new NotImplementedException();
        }
    }
}