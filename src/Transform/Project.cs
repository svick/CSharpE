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

        private readonly Transformer<TransformProject> transformer;

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
            transformer = Transformer.Create(transformations);
        }

        public Project Transform()
        {
            var projectDiff = Diff();

            transformer.Transform(projectDiff);

            Snapshot();

            return projectDiff.GetProject();
        }

        private ProjectDiff Diff() => new ProjectDiff(this);

        private void Snapshot()
        {
            foreach (var file in SourceFiles)
            {
                file.Snapshot();
            }
        }
    }
}