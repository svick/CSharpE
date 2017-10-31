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
    public class TransformProject : Project
    {
        private readonly List<SourceFile> additionalSourceFiles;

        protected override IEnumerable<SourceFile> ActualSourceFiles => SourceFiles.Concat(additionalSourceFiles);

        public TransformProject(IEnumerable<SourceFile> inputFiles, IEnumerable<Reference> additionalReferences)
            : base(inputFiles.Where(f => Path.GetExtension(f.Path) == ".cse"), additionalReferences)
        {
            additionalSourceFiles = inputFiles.Except(SourceFiles).ToList();
        }

        public TransformProject(IEnumerable<SourceFile> sourceFiles) : this(sourceFiles, Array.Empty<Reference>()) { }

        public TransformProject(Project project) : this(project.SourceFiles, project.References) { }
    }
}