using System;

namespace CSharpE.Transform.Transformers
{
    internal class DescendantTransformer<TAncestor, TDescendant> : Transformer<TAncestor>
    {
        private readonly Func<TAncestor, TDescendant> getDescendantFunction;
        private readonly Transformer<TDescendant> descendantTransformer;

        public DescendantTransformer(
            Func<TAncestor, TDescendant> descendantFunction, Transformer<TDescendant> descendantTransformer)
        {
            getDescendantFunction = descendantFunction;
            this.descendantTransformer = descendantTransformer;
        }

        // TODO: proper implementation
        private Diff<TDescendant> GetDescendantDiff(Diff<TAncestor> diff) =>
            TrivialDiff.Create(getDescendantFunction(diff.GetNew()));

        public override bool InputChanged(Diff<TAncestor> diff) =>
            descendantTransformer.InputChanged(GetDescendantDiff(diff));

        public override void Transform(TransformProject project, Diff<TAncestor> diff) =>
            descendantTransformer.Transform(project, GetDescendantDiff(diff));
    }

    internal static class DescendantTransformer
    {
        public static DescendantTransformer<TAncestor, TDescendant> Create<TAncestor, TDescendant>(
            Func<TAncestor, TDescendant> descendantFunction, Transformer<TDescendant> descendantTransformer) =>
            new DescendantTransformer<TAncestor, TDescendant>(descendantFunction, descendantTransformer);
    }
}