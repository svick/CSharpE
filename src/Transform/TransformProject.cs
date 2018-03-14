using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CSharpE.Syntax;

namespace CSharpE.Transform
{
    /// <summary>
    /// Project that hides source files that cannot be transformed.
    /// </summary>
    public class TransformProject : Syntax.Project
    {
        private readonly List<Syntax.SourceFile> additionalSourceFiles;

        protected override IEnumerable<Syntax.SourceFile> ActualSourceFiles => SourceFiles.Concat(additionalSourceFiles);

        public TransformProject(
            IEnumerable<Syntax.SourceFile> inputFiles, IEnumerable<LibraryReference> additionalReferences)
            : this(inputFiles.ToList(), additionalReferences) { }

        private TransformProject(List<Syntax.SourceFile> inputFiles, IEnumerable<LibraryReference> additionalReferences)
            : base(inputFiles.Where(f => Path.GetExtension(f.Path) == ".cse"), additionalReferences)
        {
            additionalSourceFiles = inputFiles.Except(SourceFiles).ToList();
        }

        public TransformProject(IEnumerable<Syntax.SourceFile> sourceFiles) : this(sourceFiles, Array.Empty<LibraryReference>()) { }

        public TransformProject(Syntax.Project project) : this(project.SourceFiles, project.References) { }
    }
}