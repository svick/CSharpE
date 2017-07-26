using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;

namespace CSharpE.Syntax
{
    public class Project
    {
        public IList<SourceFile> SourceFiles { get; }

        private CSharpCompilation compilation;
        
        public CSharpCompilation Compilation
        {
            get
            {
                if (compilation == null)
                    compilation = CSharpCompilation.Create(null, SourceFiles.Select(file => file.Tree));
                
                return compilation;
            }
        }

        public Project() => SourceFiles = new List<SourceFile>();
        
        public Project(IEnumerable<SourceFile> sourceFiles) => SourceFiles = sourceFiles.ToList();

        public Project(params SourceFile[] sourceFiles)
            : this(sourceFiles.AsEnumerable())
        { }

        public IEnumerable<TypeDefinition> TypesWithAttribute<T>() where T : Attribute
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
    }
}