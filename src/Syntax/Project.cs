using System.Collections.Generic;
using System.Linq;

namespace CSharpE.Syntax
{
    public class Project
    {
        public IList<SourceFile> SourceFiles { get; }

        public Project() => SourceFiles = new List<SourceFile>();

        public Project(IEnumerable<SourceFile> sourceFiles) => SourceFiles = sourceFiles.ToList();
    }
}