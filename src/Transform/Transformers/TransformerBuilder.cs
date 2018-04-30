using System;
using System.Collections.Generic;
using CSharpE.Transform.Internals;

namespace CSharpE.Transform.Transformers
{
    internal abstract class TransformerBuilder
    {
        public abstract void Collection<TParent, TItem, TData>(
            TParent parent, Func<TParent, IEnumerable<TItem>> collectionFunction, Action<TData, TItem> action, TData data);
    }

    internal class TransformerBuilder<TInput> : TransformerBuilder
    {
        private readonly TransformProject project;
        private readonly TInput input;

        public TransformerBuilder(TransformProject project, TInput input)
        {
            this.project = project;
            this.input = input;
        }

        public List<Transformer<TInput>> Transformers { get; } = new List<Transformer<TInput>>();

        public override void Collection<TParent, TItem, TData>(
            TParent parent, Func<TParent, IEnumerable<TItem>> collectionFunction, Action<TData, TItem> action, TData data)
        {
            var transformer = new CollectionTransformer<TItem, TData>();

            transformer.Transform(project, TrivialDiff.Create(collectionFunction(parent)));

            var getParentFunction = DescendantFinder.Create(input, parent);
            Func<TInput, IEnumerable<TItem>> getCollectionFunction = i => collectionFunction(getParentFunction(i));

            // TODO: getCollectionFunction might need to be split back, so that DescendantTransformer can create diff
            Transformers.Add(DescendantTransformer.Create(getCollectionFunction, transformer));
        }
    }

    internal class CollectionTransformer<TItem, TData> : Transformer<IEnumerable<TItem>>
    {
        // TODO: proper implementation
        public override bool InputChanged(Diff<IEnumerable<TItem>> diff) => true;

        public override void Transform(TransformProject project, Diff<IEnumerable<TItem>> diff)
        {
            throw new NotImplementedException();
        }
    }
}