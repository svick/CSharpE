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
            IEnumerable<Syntax.SourceFile> sourceFiles, IEnumerable<LibraryReference> additionalReferences)
            : this(sourceFiles.ToList(), additionalReferences) { }

        private TransformProject(List<Syntax.SourceFile> sourceFiles, IEnumerable<LibraryReference> additionalReferences)
            : base(sourceFiles.Where(f => Path.GetExtension(f.Path) == ".cse"), additionalReferences)
        {
            additionalSourceFiles = sourceFiles.Except(SourceFiles).ToList();
        }

        public TransformProject(IEnumerable<Syntax.SourceFile> sourceFiles) : this(sourceFiles, Array.Empty<LibraryReference>()) { }

        public TransformProject(Syntax.Project project) : this(project.SourceFiles, project.References) { }
    }
}