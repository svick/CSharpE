using System;
using System.Collections.Generic;
using System.Linq;
using CSharpE.Syntax;
using CSharpE.Transform.Execution;
using CSharpE.Transform.Internals;
using CSharpE.Transform.Transformers;

namespace CSharpE.Transform
{
    internal class TransformProject : Syntax.Project
    {
        private readonly Action<LogAction> onLog;

        private readonly List<Syntax.SourceFile> additionalSourceFiles;

        protected override IEnumerable<Syntax.SourceFile> ActualSourceFiles => SourceFiles.Concat(additionalSourceFiles);

        public TransformProject(
            IEnumerable<Syntax.SourceFile> sourceFiles, IEnumerable<LibraryReference> additionalReferences,
            Action<LogAction> onLog = null)
            : this(sourceFiles.ToList(), additionalReferences)
        {
            this.onLog = onLog;
        }

        private TransformProject(List<Syntax.SourceFile> sourceFiles, IEnumerable<LibraryReference> additionalReferences)
            : base(sourceFiles, additionalReferences)
        {
            additionalSourceFiles = sourceFiles.Except(SourceFiles).ToList();
        }

        public TransformProject(IEnumerable<Syntax.SourceFile> sourceFiles) : this(sourceFiles, Array.Empty<LibraryReference>()) { }

        public TransformProject(Syntax.Project project) : this(project.SourceFiles, project.References) { }

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