using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace CSharpE.Transform
{
    public class Project
    {
        public IList<SourceFile> SourceFiles { get; }

        private readonly ImmutableArray<ITransformation> transformations;

        public Project(IEnumerable<SourceFile> sourceFiles, IEnumerable<ITransformation> transformations)
        {
            this.SourceFiles = sourceFiles.ToList();
            this.transformations = transformations.ToImmutableArray();
        }

        public Project Transform()
        {
            Syntax.Project syntaxProject = ToSyntaxProject();

            foreach (var transformation in transformations)
            {
                transformation.Process(syntaxProject);
            }

            return FromSyntaxProject(syntaxProject);
        }

        private Syntax.Project ToSyntaxProject() => new TransformProject(
            SourceFiles.Select(f => f.ToSyntaxSourceFile()), transformations.SelectMany(t => t.AdditionalReferences));

        private Project FromSyntaxProject(Syntax.Project syntaxProject) => new Project(
            syntaxProject.SourceFiles.Select(SourceFile.FromSyntaxSourceFile), Enumerable.Empty<ITransformation>());
    }
}