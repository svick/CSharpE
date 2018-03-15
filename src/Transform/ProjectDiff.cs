using System.Linq;

namespace CSharpE.Transform
{
    // TODO: proper implementation
    internal class ProjectDiff
    {
        private readonly Syntax.Project syntaxProject;

        public ProjectDiff(Project project) => syntaxProject = new TransformProject(project.SourceFiles.Select(f => f.ToSyntaxSourceFile()));

        public Project GetProject() => new Project(
            syntaxProject.SourceFiles.Select(SourceFile.FromSyntaxSourceFile), Enumerable.Empty<ITransformation>());

        public Syntax.Project GetSyntaxProject() => syntaxProject;
    }
}