using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using CSharpSyntaxFactory = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace CSharpE.Syntax
{
    public class SourceFile : ITypeContainer
    {
        public string Path { get; }

        private CSharpSyntaxTree tree;

        public string GetText() => GetWrapped().ToString();

        public Project Project { get; internal set; }

        private SemanticModel semanticModel;
        internal SemanticModel SemanticModel
        {
            get
            {
                if (semanticModel == null)
                    semanticModel = Project.Compilation.GetSemanticModel(GetWrapped());
                
                return semanticModel;
            }
        }

        internal SyntaxContext Context => new SyntaxContext(SemanticModel);

        private List<TypeDefinition> types; // TODO: wrapper list over (string namespace, TypeDeclarationSyntax)
        public IList<TypeDefinition> Types
        {
            get
            {
                if (types == null)
                    types = tree.GetCompilationUnitRoot()
                        .DescendantNodes(node => node is CompilationUnitSyntax || node is NamespaceDeclarationSyntax)
                        .OfType<TypeDeclarationSyntax>()
                        .Select(tds => new TypeDefinition(tds, this))
                        .ToList();
                
                return types;
            }
            set => types = value.ToList();
        }

        IEnumerable<TypeDefinition> ITypeContainer.Types => Types;

        public IEnumerable<TypeDefinition> AllTypes
        {
            get
            {
                IEnumerable<TypeDefinition> AllNestedTypes(ITypeContainer container)
                {
                    foreach (var directType in container.Types)
                    {
                        yield return directType;
                        
                        foreach (var indirectType in AllNestedTypes(directType))
                        {
                            yield return indirectType;
                        }
                    }
                }

                return AllNestedTypes(this);
            }
        }

        private SourceFile(string path, SyntaxTree syntaxTree)
        {
            Path = path;
            tree = (CSharpSyntaxTree)syntaxTree;
        }

        public SourceFile(string path, string text)
            : this(path, CSharpSyntaxTree.ParseText(text))
        {
        }

        public SourceFile(string path)
        {
            Path = path;
            Types = new List<TypeDefinition>();
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

        private static readonly Func<SourceFile, CSharpSyntaxTree> TreeGenerator = self =>
        {
            var fileMembers = new List<MemberDeclarationSyntax>();

            string currentNamespace = null;
            var currentNamespaceTypes = new List<TypeDeclarationSyntax>();

            void FinishCurrentNamespace()
            {
                if (currentNamespace == null)
                    fileMembers.AddRange(currentNamespaceTypes);
                else
                    fileMembers.Add(
                        CSharpSyntaxFactory.NamespaceDeclaration(CSharpSyntaxFactory.ParseName(currentNamespace))
                            .AddMembers(currentNamespaceTypes.ToArray<MemberDeclarationSyntax>()));

                currentNamespaceTypes.Clear();
                currentNamespace = null;
            }

            foreach (var type in self.Types)
            {
                if (type.Namespace != currentNamespace)
                    FinishCurrentNamespace();

                currentNamespace = type.Namespace;
                currentNamespaceTypes.Add(type.GetWrapped());
            }

            FinishCurrentNamespace();

            return (CSharpSyntaxTree)CSharpSyntaxFactory.SyntaxTree(CSharpSyntaxFactory.CompilationUnit().AddMembers(fileMembers.ToArray()));
        };

        public CSharpSyntaxTree GetWrapped()
        {
            throw new NotImplementedException();
        }

        /*
         *         private static ClassDeclarationSyntax CreateSyntax(
            string name, SyntaxList<MemberDeclarationSyntax> membersSyntax) =>
            CSharpSyntaxFactory.ClassDeclaration(name).WithMembers(membersSyntax);

        private static readonly Func<TypeDefinition, TypeDeclarationSyntax> SyntaxNodeGenerator = self =>
        {
            var membersSyntax = self.members == null
                ? self.syntaxNode.Members
                : CSharpSyntaxFactory.List(self.members.Select(m => m.GetSyntax()));

            return CreateSyntax(self.Name, membersSyntax);
        };

        public new TypeDeclarationSyntax GetSyntax() => wrapperHelper.GetSyntaxNode(ref syntaxNode, this, SyntaxNodeGenerator);

        public new TypeDeclarationSyntax GetChangedSyntaxOrNull()
        {
            var membersSyntax = members?.GetChangedSyntaxOrNull();

            if (membersSyntax == null && !wrapperHelper.Changed)
                return syntaxNode;
            
            wrapperHelper.ResetChanged();

            return CreateSyntax(Name, membersSyntax ?? syntaxNode.Members);
        }
*/
    }

    public class NamespaceDefinition
    {
    }
}
 