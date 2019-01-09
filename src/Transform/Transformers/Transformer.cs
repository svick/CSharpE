namespace CSharpE.Transform.Transformers
{
    internal abstract class Transformer { }

    internal abstract class Transformer<TInput, TOutput> : Transformer
    {
        public abstract TOutput Transform(TransformProject project, TInput input);
    }
}