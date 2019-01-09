using System;
using System.Collections.Generic;
using System.Linq;
using CSharpE.Syntax;
using CSharpE.Transform.Execution;
using CSharpE.Transform.Transformers;
using Microsoft.CodeAnalysis.CSharp;

namespace CSharpE.Transform
{
    internal sealed class TransformProject : Project
    {
        private readonly Action<LogAction> onLog;

        public TransformProject(
            IEnumerable<SourceFile> sourceFiles, IEnumerable<LibraryReference> references,
            Action<LogAction> onLog = null)
            : this(sourceFiles.ToList(), references, null, onLog) { }

        internal TransformProject(
            IEnumerable<SourceFile> sourceFiles, IEnumerable<LibraryReference> references,
            CSharpCompilation compilation, Action <LogAction> onLog = null)
            : base(sourceFiles.ToList(), references, compilation)
        {
            this.onLog = onLog;
        }

        public TransformProject(Project project, Action<LogAction> onLog = null)
            : this(
                project.SourceFiles.Select(sf => new SourceFile(sf.Path, sf.GetSyntaxTree())),
                project.References, project.compilation)
        {
            this.onLog = onLog;
        }

        internal TransformerBuilder TransformerBuilder { get; set; }

        internal void Log(string targetKind, string targetName, string action) =>
            onLog?.Invoke((targetKind, targetName, action));
    }
}