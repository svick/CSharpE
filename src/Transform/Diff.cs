namespace CSharpE.Transform
{
    internal abstract class Diff<T>
    {
        public abstract T GetNew();
    }

    // TODO: remove?
    internal class TrivialDiff<T> : Diff<T>
    {
        private readonly T newValue;

        public TrivialDiff(T newValue) => this.newValue = newValue;

        public override T GetNew() => newValue;
    }

    internal static class TrivialDiff
    {
        public static TrivialDiff<T> Create<T>(T newValue) => new TrivialDiff<T>(newValue);
    }
}