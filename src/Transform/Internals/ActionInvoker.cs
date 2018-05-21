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

        public static ActionInvoker<T1, T2> Create(Action<T1, T2> action) => new SimpleActionInvoker<T1, T2>(action);
    }

    internal static class ActionInvoker<T>
    {
        public static ActionInvoker<Unit, T> Create(Action<T> action) => new UnitActionInvoker<T>(action);
    }

    internal class SimpleActionInvoker<T1, T2> : ActionInvoker<T1, T2>
    {
        private readonly Action<T1, T2> action;

        public SimpleActionInvoker(Action<T1, T2> action)
        {
            Debug.Assert(!ClosureChecker.HasClosure(action));

            this.action = action;
        }

        public override bool Equals(ActionInvoker<T1, T2> other)
        {
            if (ReferenceEquals(this, other))
                return true;

            var otherInvoker = other as SimpleActionInvoker<T1, T2>;

            if (otherInvoker == null)
                return false;

            return action.Method == otherInvoker.action.Method;
        }

        public override int GetHashCode() => action.Method.GetHashCode();

        public override void Invoke(T1 arg1, T2 arg2) => action(arg1, arg2);
    }

    internal class UnitActionInvoker<T> : ActionInvoker<Unit, T>
    {
        private readonly Action<T> action;

        public UnitActionInvoker(Action<T> action)
        {
            Debug.Assert(!ClosureChecker.HasClosure(action));

            this.action = action;
        }

        public override bool Equals(ActionInvoker<Unit, T> other)
        {
            if (ReferenceEquals(this, other))
                return true;

            var otherInvoker = other as UnitActionInvoker<T>;

            if (otherInvoker == null)
                return false;

            return action.Method == otherInvoker.action.Method;
        }

        public override int GetHashCode() => action.Method.GetHashCode();

        public override void Invoke(Unit arg1, T arg2) => action(arg2);
    }
}