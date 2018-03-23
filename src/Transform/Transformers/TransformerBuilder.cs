using System;
using System.Collections.Generic;
using System.Linq;
using CSharpE.Transform.Transformers;

namespace CSharpE.Transform
{
    internal abstract class TransformerBuilder
    {
        public void Collection<T>(Func<Syntax.Project, IEnumerable<T>> collectionFunc, Action<T> action)
        {
            throw new NotImplementedException();
        }
    }

    internal class TransformerBuilder<TInput> : TransformerBuilder
    {
        public List<Transformer<TInput>> Transformers { get; } = new List<Transformer<TInput>>();
    }
}