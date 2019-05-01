using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace CSharpE.Extensions.Json
{
    // https://stackoverflow.com/a/1890230/41071 and https://stackoverflow.com/a/41169463/41071
    public class ReferenceEqualityComparer<T> : EqualityComparer<T>
        where T : class
    {
        public new static ReferenceEqualityComparer<T> Default { get; } = new ReferenceEqualityComparer<T>();

        public override bool Equals(T x, T y) => ReferenceEquals(x, y);

        public override int GetHashCode(T obj) => RuntimeHelpers.GetHashCode(obj);
    }
}