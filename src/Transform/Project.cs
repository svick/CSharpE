using System;
using System.Collections.Generic;
using System.Linq;
using CSharpE.Syntax;
using CSharpE.Transform.Transformers;

namespace CSharpE.Transform
{
    public class Project
    {
        public IList<SourceFile> SourceFiles { get; }
        public IList<LibraryReference> AdditionalReferences { get; }

        private readonly List<TransformationTransformer> transformers;

        public Project(
            IEnumerable<SourceFile> sourceFiles, IEnumerable<Type> additionalReferencesRepresentatives,
            IEnumerable<ITransformation> transformations)
            : this(
                sourceFiles, additionalReferencesRepresentatives.Select(t => new AssemblyReference(t)),
                transformations) { }

        public Project(
            IEnumerable<SourceFile> sourceFiles, IEnumerable<LibraryReference> additionalReferences,
            IEnumerable<ITransformation> transformations)
        {
            SourceFiles = sourceFiles.ToList();
            AdditionalReferences = additionalReferences.ToList();
            transformers = transformations.Select(t => new TransformationTransformer(t)).ToList();
        }

        public Project Transform()
        {
            var transformProject = new TransformProject(
                SourceFiles.Select(f => f.ToSyntaxSourceFile()), AdditionalReferences);

            foreach (var transformer in transformers)
            {
                transformer.Transform(transformProject);
            }

            return new Project(
                transformProject.SourceFiles.Select(SourceFile.FromSyntaxSourceFile),
                Enumerable.Empty<LibraryReference>(), Enumerable.Empty<ITransformation>());
        }
    }
}