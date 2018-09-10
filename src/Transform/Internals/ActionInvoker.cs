using System;
using System.Diagnostics;

namespace CSharpE.Transform.Internals
{
    // TODO: probably split into invoker and combiner; if not, at least rename
    internal class ActionInvoker<T1, T2, TIntermediate, TResult>
        : IEquatable<ActionInvoker<T1, T2, TIntermediate, TResult>>
    {
        private readonly Func<T1, T2, TIntermediate> action;
        private readonly Func<TResult, TIntermediate, TResult> combine;
        private readonly object methods;

        private TResult state;

        public ActionInvoker(
            Func<T1, T2, TIntermediate> action, Func<TResult, TIntermediate, TResult> combine, object methods)
        {
            this.action = action;
            this.combine = combine;
            this.methods = methods;
        }

        public bool Equals(ActionInvoker<T1, T2, TIntermediate, TResult> other)
        {
            if (other is null)
                return false;

            if (ReferenceEquals(this, other))
                return true;

            return Equals(methods, other.methods);
        }

        public override bool Equals(object obj) => Equals(obj as ActionInvoker<T1, T2, TIntermediate, TResult>);

        public override int GetHashCode() => methods.GetHashCode();

        public TIntermediate Invoke(T1 arg1, T2 arg2) => action(arg1, arg2);

        public void ProvideIntermediate(TIntermediate value) => state = combine(state, value);

        public TResult GetResult()
        {
            var result = state;
            state = default;
            return result;
        }
        
    }

    internal static class ActionInvoker
    {
        public static ActionInvoker<Unit, T, Unit, Unit> Create<T>(Action<T> action)
        {
            return new ActionInvoker<Unit, T, Unit, Unit>((_, x) =>
            {
                action(x);
                return Unit.Value;
            }, (_, __) => Unit.Value, action.Method);
        }
        
        public static ActionInvoker<T1, T2, Unit, Unit> Create<T1, T2>(Action<T1, T2> action)
        {
            return new ActionInvoker<T1, T2, Unit, Unit>((arg1, arg2) =>
            {
                action(arg1, arg2);
                return Unit.Value;
            }, (_, __) => Unit.Value, action.Method);
        }

        public static ActionInvoker<Unit, T, TIntermediate, TResult> Create<T, TIntermediate, TResult>(
            Func<T, TIntermediate> action, Func<TResult, TIntermediate, TResult> combine)
        {
            Debug.Assert(!ClosureChecker.HasClosure(combine));

            return new ActionInvoker<Unit, T, TIntermediate, TResult>(
                (_, x) => action(x), combine, (action.Method, combine.Method));
        }
        
        public static ActionInvoker<T1, T2, TIntermediate, TResult> Create<T1, T2, TIntermediate, TResult>(
            Func<T1, T2, TIntermediate> action, Func<TResult, TIntermediate, TResult> combine)
        {
            Debug.Assert(!ClosureChecker.HasClosure(combine));

            return new ActionInvoker<T1, T2, TIntermediate, TResult>(action, combine, (action.Method, combine.Method));
        }
    }
}