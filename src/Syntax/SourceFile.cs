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
    public class SourceFile : SyntaxNode, ITypeContainer
    {
        public string Path { get; }

        private SyntaxTree syntax;

        public SourceFile(string path, SyntaxTree syntaxTree)
        {
            Path = path;
            syntax = syntaxTree;
        }

        public SourceFile(string path, string text)
            : this(path, CSharpSyntaxTree.ParseText(text)) { }

        public SourceFile(string path)
        {
            Path = path;
            members = new SyntaxList<NamespaceOrTypeDefinition, MemberDeclarationSyntax>();
        }

        public string GetText() => GetWrapped().ToString();

        public Project Project { get; internal set; }

        private SemanticModel semanticModel;
        internal SemanticModel SemanticModel
        {
            get
            {
                if (semanticModel == null)
                {
                    if (Project == null)
                        throw new InvalidOperationException("SourceFile has to be part of a Project for this operation.");

                    semanticModel = Project.Compilation.GetSemanticModel(GetWrapped());
                }

                return semanticModel;
            }
        }

        internal SyntaxContext SyntaxContext => new SyntaxContext(SemanticModel);

        private SyntaxList<NamespaceOrTypeDefinition, MemberDeclarationSyntax> members;
        public IList<NamespaceOrTypeDefinition> Members
        {
            get
            {
                if (members == null)
                {
                    members = new SyntaxList<NamespaceOrTypeDefinition, MemberDeclarationSyntax>(
                        syntax.GetCompilationUnitRoot().Members, mds =>
                        {
                            switch (mds)
                            {
                                case NamespaceDeclarationSyntax ns: return new NamespaceDefinition(ns);
                                case TypeDeclarationSyntax type: return new TypeDefinition(type, this);
                                default: throw new InvalidOperationException();
                            }
                        });
                }

                return members;
            }
        }

        // TODO: virtual list
        public IEnumerable<TypeDefinition> Types  
        {
            get
            {
                IEnumerable<TypeDefinition> GetTypes(NamespaceOrTypeDefinition container)
                {
                    if (container.IsNamespace)
                    {
                        return container.GetNamespaceDefinition().Members.SelectMany(GetTypes);
                    }
                    else
                    {
                        return new[] { container.GetTypeDefinition() };
                    }
                }

                return Members.SelectMany(GetTypes);
            }
        }

        IEnumerable<TypeDefinition> ITypeContainer.Types => Types;

        public IEnumerable<TypeDefinition> AllTypes
        {
            get
            {
                IEnumerable<TypeDefinition> GetAllTypes(ITypeContainer container)
                {
                    foreach (var directType in container.Types)
                    {
                        yield return directType;
                        
                        foreach (var indirectType in GetAllTypes(directType))
                        {
                            yield return indirectType;
                        }
                    }
                }

                return GetAllTypes(this);
            }
        }

        private readonly HashSet<string> additionalNamespaces = new HashSet<string>();

        public static async Task<SourceFile> OpenAsync(string path)
        {
            using (var stream = File.OpenRead(path))
            {
                // there is no async version of SourceText.From: https://github.com/dotnet/roslyn/issues/20796
                var syntaxTree = CSharpSyntaxTree.ParseText(await Task.Run(() => SourceText.From(stream)));
                
                return new SourceFile(path, syntaxTree);
            }
        }

        public void EnsureUsingNamespace(string ns)
        {
            additionalNamespaces.Add(ns);
        }

        public SyntaxTree GetWrapped()
        {
            var oldCompilationUnit = syntax?.GetCompilationUnitRoot();
            var oldUsings = oldCompilationUnit?.Usings ?? default;

            var oldUsingNamespaces = oldUsings
                .Where(u => u.Alias == null && u.StaticKeyword.IsKind(SyntaxKind.None))
                .Select(u => u.Name)
                .ToList();

            // remove additional names that already have a using
            additionalNamespaces.RemoveWhere(
                ns =>
                {
                    var nameSyntax = CSharpSyntaxFactory.ParseName(ns);
                    return oldUsingNamespaces.Any(ons => nameSyntax.IsEquivalentTo(ons));
                });

            var newMembers = members?.GetWrapped(new WrapperContext(this)) ?? oldCompilationUnit.Members;

            if (syntax == null || additionalNamespaces.Any() || newMembers != oldCompilationUnit.Members)
            {
                var newUsings = oldUsings;

                if (additionalNamespaces.Any())
                {
                    // TODO: sort usings?
                    newUsings = newUsings.AddRange(
                        additionalNamespaces.OrderBy(x => x).Select(
                            ns => CSharpSyntaxFactory.UsingDirective(CSharpSyntaxFactory.ParseName(ns))));
                }

                syntax = CSharpSyntaxFactory.SyntaxTree(
                    CSharpSyntaxFactory.CompilationUnit(default, newUsings, default, newMembers).NormalizeWhitespace());

                additionalNamespaces.Clear();
            }

            return syntax;
        }

        protected override IEnumerable<IEnumerable<SyntaxNode>> GetChildren()
        {
            yield return Members.Select(m => m.Value);
        }

        public override SyntaxNode Parent
        {
            get => null;
            internal set => throw new InvalidOperationException();
        }
    }
}
 