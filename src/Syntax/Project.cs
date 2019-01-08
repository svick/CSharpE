﻿using System.Collections.Generic;
using System.Linq;
using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis.CSharp;

namespace CSharpE.Syntax
{
    public class Project
    {
        public IList<SourceFile> SourceFiles { get; }

        public IList<LibraryReference> References { get; }

        private CSharpCompilation compilation;
        public CSharpCompilation Compilation
        {
            get
            {
                // TODO: if References changed, alter the compilation

                var trees = SourceFiles.Select(file => file.GetSyntaxTree()).ToList();

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


        public Project(IEnumerable<SourceFile> sourceFiles, IEnumerable<LibraryReference> references)
            : this(sourceFiles, references, null) { }

        internal Project(IEnumerable<SourceFile> sourceFiles, IEnumerable<LibraryReference> references, CSharpCompilation compilation)
        {
            SourceFiles = sourceFiles.ToList();

            foreach (var sourceFile in SourceFiles)
            {
                sourceFile.Project = this;
            }

            References = references.ToList();

            this.compilation = compilation;
        }

        // TODO: limit type kind and use in ActorTransformation
        public IEnumerable<BaseTypeDefinition> GetTypesWithAttribute<T>() where T : System.Attribute =>
            NestedCollection.Create(this, SourceFiles, sourceFile => sourceFile.GetTypesWithAttribute<T>());

        public IEnumerable<BaseTypeDefinition> GetTypes() =>
            NestedCollection.Create(this, SourceFiles, sourceFile => sourceFile.GetTypes());

        public IEnumerable<ClassDefinition> GetClasses() =>
            NestedCollection.Create(this, SourceFiles, sourceFile => sourceFile.GetClasses());

        public IEnumerable<BaseTypeDefinition> GetAllTypes() =>
            NestedCollection.Create(this, SourceFiles, sourceFile => sourceFile.GetAllTypes());

        public IEnumerable<MethodDefinition> GetMethods() =>
            NestedCollection.Create(this, SourceFiles, sourceFile => sourceFile.GetMethods());
    }
}