using System.Collections.Generic;
using System.Linq;
using CSharpE.Syntax.Internals;

namespace CSharpE.Transform.Transformers
{
    internal abstract class Transformer
    {
        public static Transformer Create(IEnumerable<ITransformation> transformations) =>
            new SequentialTransformer(transformations.Select(Create));

        private static Transformer Create(ITransformation transformation) =>
            new TransformationTransformer(transformation);

        public abstract void Transform(ProjectDiff diff);
    }

    // TODO: proper implementation
    internal class TransformationTransformer : Transformer
    {
        private readonly ITransformation transformation;

        public TransformationTransformer(ITransformation transformation) => this.transformation = transformation;

        public override void Transform(ProjectDiff diff)
        {
            var syntaxProject = diff.GetSyntaxProject();
            syntaxProject.References.AddRange(transformation.AdditionalReferences);
            transformation.Process(syntaxProject);
        }
    }
}