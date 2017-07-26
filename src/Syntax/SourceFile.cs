using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace CSharpE.Syntax
{
    public class SourceFile
    {
        public string Path { get; }

        internal CSharpSyntaxTree Tree { get; }

        public string GetText() => Tree.ToString();

        public Project Project { get; internal set; }

        private SemanticModel semanticModel;
        internal SemanticModel SemanticModel
        {
            get
            {
                if (semanticModel == null)
                    semanticModel = Project.Compilation.GetSemanticModel(Tree);
                
                return semanticModel;
            }
        }

        public bool IsModified => throw new NotImplementedException();

        private List<TypeDefinition> types;

        public IList<TypeDefinition> Types
        {
            get
            {
                if (types == null)
                    types = Tree.GetCompilationUnitRoot()
                        .DescendantNodes(node => node is CompilationUnitSyntax || node is NamespaceDeclarationSyntax)
                        .OfType<TypeDeclarationSyntax>()
                        .Select(tds => new TypeDefinition(tds, this))
                        .ToList();
                
                return types;
            }
            set => types = value.ToList();
        }

        private SourceFile(string path, SyntaxTree syntaxTree)
        {
            Path = path;
            Tree = (CSharpSyntaxTree)syntaxTree;
        }

        public SourceFile(string path, string text)
            : this(path, CSharpSyntaxTree.ParseText(text))
        {
        }

        public static async Task<SourceFile> OpenAsync(string path)
        {
            using (var stream = File.OpenRead(path))
            {
                // there is no async version of SourceText.From: https://github.com/dotnet/roslyn/issues/20796
                var syntaxTree = CSharpSyntaxTree.ParseText(await Task.Run(() => SourceText.From(stream)));
                
                return new SourceFile(path, syntaxTree);
            }
        }
    }
}