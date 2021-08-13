using System;
using System.Diagnostics;

#nullable enable

namespace CSharpE.Syntax.Internals
{
    readonly struct Union<T1, T2> where T1 : class where T2 : class
    {
        private readonly object value;

        public Union(T1 value) => this.value = value;
        public Union(T2 value) => this.value = value;

        public TResult Switch<TResult>(Func<T1, TResult> selector1, Func<T2, TResult> selector2)
        {
            if (value is T1 value1)
                return selector1(value1);

            return selector2((T2)value);
        }

        /// <remarks>
        /// This exists, because the C# 9 compiler can't correctly infer nullability from the <c>null</c> literal.
        /// </remarks>
        public TResult? SwitchN<TResult>(Func<T1, TResult?> selector1, Func<T2, TResult?> selector2) where TResult : class
        {
            if (value is T1 value1)
                return selector1(value1);

            return selector2((T2)value);
        }

        public TBase? GetBase<TBase>()
        {
            Debug.Assert(typeof(TBase).IsAssignableFrom(typeof(T1)) && typeof(TBase).IsAssignableFrom(typeof(T1)));

            return (TBase)value;
        }

        public static implicit operator Union<T1, T2>(T1 value) => new(value);
        public static implicit operator Union<T1, T2>(T2 value) => new(value);

        public static Union<T1, T2> FromEither(object? value)
        {
            if (value is T1 value1)
                return new(value1);
            if (value is T2 value2)
                return new(value2);
            if (value is null)
                return new();

            throw new ArgumentException($"Value was expected to be of type {typeof(T1)} or {typeof(T2)}, but it was of type {value.GetType()}.");
        }
    }
}
