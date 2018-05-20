using System;
using System.Diagnostics;

namespace CSharpE.Transform.Internals
{
    internal abstract class ActionInvoker<T1, T2> : IEquatable<ActionInvoker<T1, T2>>
    {
        public abstract bool Equals(ActionInvoker<T1, T2> other);

        public abstract override int GetHashCode();

        public sealed override bool Equals(object obj) => Equals(obj as ActionInvoker<T1, T2>);

        public abstract void Invoke(T1 arg1, T2 arg2);

        public static ActionInvoker<T1, T2> Create<TWrapped>(
            TWrapped wrapped, Action<TWrapped, T1, T2> unwrapper = null) where TWrapped : Delegate =>
            new ActionInvoker<T1, T2, TWrapped>(wrapped, unwrapper);
    }

    internal class ActionInvoker<T1, T2, TWrapped> : ActionInvoker<T1, T2> where TWrapped : Delegate
    {
        private readonly TWrapped wrapped;
        private readonly Action<TWrapped, T1, T2> unwrapper;

        public ActionInvoker(TWrapped wrapped, Action<TWrapped, T1, T2> unwrapper = null)
        {
            Debug.Assert(!ClosureChecker.HasClosure(wrapped));

            if (unwrapper != null)
                ClosureChecker.ThrowIfHasClosure(unwrapper);

            if (unwrapper == null && !(wrapped is Action<T1, T2>))
                throw new ArgumentException(nameof(wrapped));

            this.wrapped = wrapped;
            this.unwrapper = unwrapper;
        }

        public override bool Equals(ActionInvoker<T1, T2> other)
        {
            if (ReferenceEquals(this, other))
                return true;

            var otherInvoker = other as ActionInvoker<T1, T2, TWrapped>;

            if (otherInvoker == null)
                return false;

            return wrapped.Method == otherInvoker.wrapped.Method && unwrapper?.Method == otherInvoker.unwrapper?.Method;
        }

        public override int GetHashCode() => (wrapped.Method, unwrapper?.Method).GetHashCode();

        public override void Invoke(T1 arg1, T2 arg2)
        {
            if (unwrapper == null)
            {
                var unwrapped = (Action<T1, T2>)(object)wrapped;
                unwrapped(arg1, arg2);
            }
            else
            {
                unwrapper(wrapped, arg1, arg2);
            }
        }
    }
}