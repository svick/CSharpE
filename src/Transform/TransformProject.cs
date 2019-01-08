using System;
using System.Collections.Generic;
using System.Linq;
using CSharpE.Syntax;
using CSharpE.Transform.Execution;
using CSharpE.Transform.Internals;
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
            CSharpCompilation compilation, Action < LogAction> onLog = null)
            : base(sourceFiles.ToList(), references, compilation)
        {
            this.onLog = onLog;
        }

        public TransformProject(IEnumerable<SourceFile> sourceFiles) : this(sourceFiles, Array.Empty<LibraryReference>()) { }

        public TransformProject(Project project) : this(project.SourceFiles, project.References) { }

        internal TransformerBuilder TransformerBuilder { get; set; }

        /// <summary>
        /// Runs transformation and returns a transformer that can be used to rerun the same transformation.
        /// </summary>
        public Transformer<TransformProject, Unit> RunTransformation(ITransformation transformation, bool designTime)
        {
            var transformer = CodeTransformer<TransformProject, Unit>.Create(project =>
            {
                transformation.Process(project, designTime);
                return Unit.Value;
            });

            transformer.Transform(this, this);

            return transformer;
        }

        internal void Log(string targetKind, string targetName, string action) =>
            onLog?.Invoke((targetKind, targetName, action));
    }
}