using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CSharpE.Syntax;
using CSharpE.Syntax.Internals;
using CSharpE.Transform.Transformers;

namespace CSharpE.Transform
{
    /// <summary>
    /// Project that hides source files that cannot be transformed.
    /// </summary>
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
            : base(sourceFiles.Where(f => Path.GetExtension(f.Path) == ".cse"), additionalReferences)
        {
            additionalSourceFiles = sourceFiles.Except(SourceFiles).ToList();
        }

        public TransformProject(IEnumerable<Syntax.SourceFile> sourceFiles) : this(sourceFiles, Array.Empty<LibraryReference>()) { }

        public TransformProject(Syntax.Project project) : this(project.SourceFiles, project.References) { }

        internal TransformerBuilder TransformerBuilder { get; set; }

        /// <summary>
        /// Runs transformation and returns a transformer that can be used to rerun the same transformation.
        /// </summary>
        public Transformer<TransformProject> RunTransformation(ITransformation transformation)
        {
            this.References.AddRange(transformation.AdditionalReferences);

            var transformer = CodeTransformer<TransformProject>.Create(transformation.Process);

            transformer.Transform(this, this);

            return transformer;
        }

        internal void Log(string targetKind, string targetName, string action) =>
            onLog?.Invoke((targetKind, targetName, action));
    }
}