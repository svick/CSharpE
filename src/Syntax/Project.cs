using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;

namespace CSharpE.Syntax
{
    public class Project
    {
        // TODO
        private static readonly LibraryReference[] DefaultReferences = { new AssemblyReference(typeof(object)) };
        
        public IList<SourceFile> SourceFiles { get; }

        protected virtual IEnumerable<SourceFile> ActualSourceFiles => SourceFiles;
        
        public IList<LibraryReference> References { get; }

        private CSharpCompilation compilation;
        public CSharpCompilation Compilation
        {
            get
            {
                // TODO: if References changed, alter the compilation

                var trees = ActualSourceFiles.Select(file => file.GetWrapped()).ToList();

                if (compilation == null)
                {
                    compilation = CSharpCompilation.Create(
                        null, trees, References.Select(r => r.GetMetadataReference()));
                }
                else if (!trees.SequenceEqual(compilation.SyntaxTrees))
                {
                    // PERF: only replace trees that changed, but be careful about what happens if order changes
                    compilation = compilation.RemoveAllSyntaxTrees().AddSyntaxTrees(trees);
                }

                return compilation;
            }
        }

        public Project(IEnumerable<SourceFile> sourceFiles, IEnumerable<LibraryReference> additionalReferences)
        {
            SourceFiles = sourceFiles.ToList();

            foreach (var sourceFile in SourceFiles)
            {
                sourceFile.Project = this;
            }

            References = DefaultReferences.Union(additionalReferences).ToList();
        }

        public Project(IEnumerable<SourceFile> sourceFiles)
            : this(sourceFiles, Array.Empty<LibraryReference>())
        { }

        public Project()
            : this(Array.Empty<SourceFile>())
        { }

        public Project(params SourceFile[] sourceFiles)
            : this(sourceFiles.AsEnumerable())
        { }

        public Project(IEnumerable<SourceFile> sourceFiles, IEnumerable<Type> additionalReferencesRepresentatives)
            : this(sourceFiles, additionalReferencesRepresentatives.Select(t => new AssemblyReference(t)))
        { }

        public IEnumerable<TypeDefinition> TypesWithAttribute<T>() where T : System.Attribute =>
            SourceFiles.SelectMany(sourceFile => sourceFile.TypesWithAttribute<T>());

        public IEnumerable<TypeDefinition> Types() => SourceFiles.SelectMany(sourceFile => sourceFile.Types);

        public IEnumerable<TypeDefinition> AllTypes() => SourceFiles.SelectMany(sourceFile => sourceFile.AllTypes);
    }
}