using System;
using System.Linq;
using CSharpE.Syntax;
using CSharpE.Transform.Execution;
using CSharpE.Transform.Transformers;

namespace CSharpE.Transform
{
    internal sealed class TransformProject : Project
    {
        private readonly Action<LogAction> onLog;

        public TransformProject(Project project, Action<LogAction> onLog = null)
            : base(
                project.SourceFiles.Select(sf => new SourceFile(sf.Path, sf.GetSyntaxTree())),
                project.References, project.compilation)
        {
            this.onLog = onLog;
        }

        internal TransformerCollector TransformerCollector { get; set; }

        internal void Log(string targetKind, string targetName, string action) =>
            onLog?.Invoke((targetKind, targetName, action));
    }
}