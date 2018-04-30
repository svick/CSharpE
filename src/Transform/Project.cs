using System.Collections.Generic;
using System.Linq;
using CSharpE.Transform.Transformers;

namespace CSharpE.Transform
{
    public class Project
    {
        public IList<SourceFile> SourceFiles { get; }

        private readonly Transformer<TransformProject> transformer;

        public Project(IEnumerable<SourceFile> sourceFiles, IEnumerable<ITransformation> transformations)
        {
            SourceFiles = sourceFiles.ToList();
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