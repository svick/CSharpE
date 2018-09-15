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
using Roslyn = Microsoft.CodeAnalysis;

namespace CSharpE.Syntax
{
    public class SourceFile : SyntaxNode, ISyntaxWrapper<CompilationUnitSyntax>
    {
        public string Path { get; }

        private SyntaxTree syntax;

        internal SourceFile(string path, SyntaxTree syntaxTree)
        {
            Path = path ?? throw new ArgumentNullException(nameof(path));
            syntax = syntaxTree;
        }

        public SourceFile(string path, string text)
            : this(path, CSharpSyntaxTree.ParseText(text)) { }

        public SourceFile(string path)
        {
            Path = path;
            members = new NamespaceOrTypeList(this);
        }

        public string GetText() => GetSyntaxTree().ToString();

        internal Project Project { get; set; }

        private SemanticModel semanticModel;
        internal SemanticModel SemanticModel
        {
            get
            {
                // note that GetSyntaxTree() nulls-out semanticModel if change occurred
                var syntaxTree = GetSyntaxTree();

                if (semanticModel == null)
                {
                    if (Project == null)
                        throw new InvalidOperationException("SourceFile has to be part of a Project for this operation.");

                    semanticModel = Project.Compilation.GetSemanticModel(syntaxTree);
                }

                return semanticModel;
            }
        }

        internal SyntaxContext SyntaxContext => new SyntaxContext(SemanticModel);

        private NamespaceOrTypeList members;
        public IList<NamespaceOrTypeDefinition> Members
        {
            get
            {
                if (members == null)
                {
                    members = new NamespaceOrTypeList(syntax.GetCompilationUnitRoot().Members, this);
                }

                return members;
            }
            set => SetList(ref members, new NamespaceOrTypeList(value, this));
        }

        public IEnumerable<BaseTypeDefinition> GetTypes()
        {
            IEnumerable<BaseTypeDefinition> GetTypes(NamespaceOrTypeDefinition container)
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

        public IEnumerable<ClassDefinition> GetClasses() => GetTypes().OfType<ClassDefinition>();

        public IEnumerable<BaseTypeDefinition> GetAllTypes()
        {
            IEnumerable<BaseTypeDefinition> GetAllTypes(BaseTypeDefinition type)
            {
                yield return type;

                if (type is TypeDefinition typeDefinition)
                {
                    foreach (var directType in typeDefinition.Types)
                    {
                        foreach (var indirectType in GetAllTypes(directType))
                        {
                            yield return indirectType;
                        }
                    }
                }
            }

            return GetTypes().SelectMany(GetAllTypes);
        }

        public IEnumerable<BaseTypeDefinition> GetTypesWithAttribute<T>() where T : System.Attribute
        {
            foreach (var type in GetTypes())
            {
                if (type.HasAttribute<T>())
                    yield return type;
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

        internal void EnsureUsingNamespace(string ns) => additionalNamespaces.Add(ns);

        private SyntaxTree GetWrapped(ref bool? changed)
        {
            GetAndResetChanged(ref changed);

            bool? thisChanged = false;

            var oldCompilationUnit = syntax?.GetCompilationUnitRoot();
            var oldUsings = oldCompilationUnit?.Usings ?? default;

            var oldUsingNamespaces = oldUsings
                .Where(u => u.Alias == null && u.StaticKeyword.IsKind(SyntaxKind.None))
                .Select(u => u.Name)
                .ToList();

            var newMembers = members?.GetWrapped(ref thisChanged) ?? oldCompilationUnit.Members;

            // remove additional names that already have a using
            // NOTE: this has to be executed *after* all GetWrapped() have already been called
            // (otherwise new usings could be added by those called)
            additionalNamespaces.RemoveWhere(
                ns =>
                {
                    var nameSyntax = CSharpSyntaxFactory.ParseName(ns);
                    return oldUsingNamespaces.Any(ons => nameSyntax.IsEquivalentTo(ons));
                });

            if (syntax == null || additionalNamespaces.Any() || thisChanged == true)
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

                SetChanged(ref changed);

                semanticModel = null;
            }

            return syntax;
        }

        CompilationUnitSyntax ISyntaxWrapper<CompilationUnitSyntax>.GetWrapped(ref bool? changed) =>
            GetWrapped(ref changed).GetCompilationUnitRoot();

        internal SyntaxTree GetSyntaxTree()
        {
            bool? changed = null;
            return GetWrapped(ref changed);
        }

        private protected override void SetSyntaxImpl(Roslyn::SyntaxNode newSyntax) =>
            syntax = syntax.WithRootAndOptions(newSyntax, syntax.Options);

        internal override SyntaxNode Clone() => throw new InvalidOperationException();

        internal override SyntaxNode Parent
        {
            get => null;
            set => throw new InvalidOperationException();
        }
    }
}
 