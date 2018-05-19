namespace CSharpE.Transform
{
    // TODO: proper implementation
    internal class ProjectDiff : Diff<TransformProject>
    {
        private readonly TransformProject transformProject;

        public ProjectDiff(TransformProject transformProject) => this.transformProject = transformProject;

        public override TransformProject GetNew() => transformProject;
    }
}