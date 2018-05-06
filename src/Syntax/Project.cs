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
                // TODO: if something changed, alter the compilation

                if (compilation == null)
                    compilation = CSharpCompilation.Create(
                        null, ActualSourceFiles.Select(file => file.GetWrapped()), References.Select(r => r.GetMetadataReference()));

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

        public IEnumerable<TypeDefinition> TypesWithAttribute<T>() where T : System.Attribute
        {
            foreach (var sourceFile in SourceFiles)
            {
                foreach (var type in sourceFile.Types)
                {
                    if (type.HasAttribute<T>())
                        yield return type;
                }
            }
        }

        public IEnumerable<TypeDefinition> Types()
        {
            foreach (var sourceFile in SourceFiles)
            {
                foreach (var type in sourceFile.Types)
                {
                    yield return type;
                }
            }
        }

        public IEnumerable<TypeDefinition> AllTypes()
        {
            foreach (var sourceFile in SourceFiles)
            {
                foreach (var type in sourceFile.AllTypes)
                {
                    yield return type;
                }
            }
        }
    }
}