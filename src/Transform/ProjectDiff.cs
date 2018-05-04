using System.Linq;
using CSharpE.Syntax;

namespace CSharpE.Transform
{
    // TODO: proper implementation
    internal class ProjectDiff : Diff<TransformProject>
    {
        private readonly TransformProject transformProject;

        public ProjectDiff(Project project) => transformProject = new TransformProject(project.SourceFiles.Select(f => f.ToSyntaxSourceFile()), project.AdditionalReferences);

        public Project GetProject() => new Project(
            transformProject.SourceFiles.Select(SourceFile.FromSyntaxSourceFile), Enumerable.Empty<LibraryReference>(),
            Enumerable.Empty<ITransformation>());

        public override TransformProject GetNew() => transformProject;
    }
}