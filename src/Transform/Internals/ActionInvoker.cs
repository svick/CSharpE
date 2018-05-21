using System;
using System.Diagnostics;
using System.Reflection;

namespace CSharpE.Transform.Internals
{
    internal class ActionInvoker<T1, T2> : IEquatable<ActionInvoker<T1, T2>>
    {
        private readonly Action<T1, T2> action;
        private readonly MethodInfo method;

        public ActionInvoker(Action<T1, T2> action, MethodInfo method)
        {
            this.action = action;
            this.method = method;
        }

        public bool Equals(ActionInvoker<T1, T2> other)
        {
            if (other is null)
                return false;

            if (ReferenceEquals(this, other))
                return true;

            return method == other.method;
        }

        public override bool Equals(object obj) => Equals(obj as ActionInvoker<T1, T2>);

        public override int GetHashCode() => method.GetHashCode();

        public void Invoke(T1 arg1, T2 arg2) => action(arg1, arg2);

        public static ActionInvoker<T1, T2> Create(Action<T1, T2> action)
        {
            Debug.Assert(!ClosureChecker.HasClosure(action));

            return new ActionInvoker<T1, T2>(action, action.Method);
        }
    }

    internal static class ActionInvoker<T>
    {
        public static ActionInvoker<Unit, T> Create(Action<T> action)
        {
            Debug.Assert(!ClosureChecker.HasClosure(action));

            return new ActionInvoker<Unit, T>((_, x) => action(x), action.Method);
        }
    }
}