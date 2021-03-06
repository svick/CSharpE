﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using RoslynSyntaxFactory = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Roslyn = Microsoft.CodeAnalysis;

namespace CSharpE.Syntax
{
    public sealed class SourceFile : SyntaxNode, ISyntaxWrapper<CompilationUnitSyntax>
    {
        public string Path { get; }

        private SyntaxTree syntax;

        internal SourceFile(string path, SyntaxTree syntaxTree)
        {
            Path = path ?? throw new ArgumentNullException(nameof(path));
            syntax = syntaxTree;
        }

        internal SourceFile(SyntaxTree syntaxTree) : this(syntaxTree.FilePath, syntaxTree) { }

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
                    members = new NamespaceOrTypeList(syntax.GetCompilationUnitRoot().Members, this);

                return ProjectionList.Create(
                    members, member => new NamespaceOrTypeDefinition(member), notd => notd.NamespaceOrType);
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

            return SimpleCollection.Create(this, Members.SelectMany(GetTypes));
        }

        public IEnumerable<ClassDefinition> GetClasses() =>
            SimpleCollection.Create(this, GetTypes().OfType<ClassDefinition>());

        public IEnumerable<BaseTypeDefinition> GetAllTypes()
        {
            IEnumerable<BaseTypeDefinition> GetAllTypes(IEnumerable<BaseTypeDefinition> types)
            {
                foreach (var type in types)
                {
                    yield return type;

                    if (type is TypeDefinition typeDefinition)
                    {
                        foreach (var descendantType in GetAllTypes(typeDefinition.Types))
                        {
                            yield return descendantType;
                        }
                    }
                }
            }

            return SimpleCollection.Create(this, GetAllTypes(GetTypes()));
        }

        public IEnumerable<ClassDefinition> GetClassesWithAttribute<T>() where T : System.Attribute =>
            SimpleCollection.Create(this, GetClasses().Where(type => type.HasAttribute<T>()));

        public IEnumerable<BaseTypeDefinition> GetTypesWithAttribute<T>() where T : System.Attribute =>
            SimpleCollection.Create(this, GetTypes().Where(type => type.HasAttribute<T>()));

        public IEnumerable<MethodDefinition> GetMethods() => NestedCollection.Create(
            this, GetTypes(), baseType =>
            {
                if (baseType is TypeDefinition type)
                    return type.Methods;

                return SimpleCollection.Create(baseType, Enumerable.Empty<MethodDefinition>());
            });

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

        private readonly List<UsingNamespaceRecorder> usingNamespaceRecorders = new List<UsingNamespaceRecorder>();

        internal void EnsureUsingNamespace(string ns)
        {
            if (string.IsNullOrEmpty(ns))
                return;

            foreach (var recorder in usingNamespaceRecorders)
            {
                recorder.Record(ns);
            }

            additionalNamespaces.Add(ns);
        }

        internal UsingNamespaceRecorder RecordUsingNamespaces()
        {
            var recorder = new UsingNamespaceRecorder(this);

            usingNamespaceRecorders.Add(recorder);

            return recorder;
        }

        internal class UsingNamespaceRecorder
        {
            private readonly SourceFile sourceFile;
            private readonly HashSet<string> namespaces = new HashSet<string>();

            public UsingNamespaceRecorder(SourceFile sourceFile) => this.sourceFile = sourceFile;

            internal void Record(string ns) => namespaces.Add(ns);

            internal IEnumerable<string> StopAndGetResult()
            {
                sourceFile.usingNamespaceRecorders.Remove(this);

                return namespaces;
            }
        }

        private SyntaxTree GetWrapped(ref bool? changed)
        {
            GetAndResetChanged(ref changed, out var thisChanged);

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
                    var nameSyntax = RoslynSyntaxFactory.ParseName(ns);
                    return oldUsingNamespaces.Any(ons => nameSyntax.IsEquivalentTo(ons));
                });

            if (syntax == null || additionalNamespaces.Any() || thisChanged == true)
            {
                var newUsings = oldUsings;

                if (additionalNamespaces.Any())
                {
                    newUsings = RoslynSyntaxFactory.List(
                        newUsings
                            .AddRange(
                                additionalNamespaces.Select(ns => RoslynSyntaxFactory.UsingDirective(RoslynSyntaxFactory.ParseName(ns))))
                            .OrderBy(u => u.Alias != null)
                            .ThenBy(u => u.StaticKeyword != default)
                            .ThenByDescending(u => u.Name.ToString().StartsWith("System"))
                            .ThenBy(u => u.Name.ToString()));
                }

                // Compilation requires the same LanguageVersion on all SyntaxTrees
                var parseOptions = Project?.compilation == null
                    ? null
                    : new CSharpParseOptions(Project.compilation.LanguageVersion);

                syntax = RoslynSyntaxFactory.SyntaxTree(
                    RoslynSyntaxFactory.CompilationUnit(default, newUsings, default, newMembers).NormalizeWhitespace(),
                    parseOptions, Path);

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

        private protected override SyntaxNode CloneImpl()
        {
            // preserve syntax tree (except members) if it exists, to make sure using static and using alias directives survive
            return new SourceFile(Path, syntax?.WithRootAndOptions(syntax.GetCompilationUnitRoot().WithMembers(default), syntax.Options))
            {
                Members = Members
            };
        }

        public override IEnumerable<SyntaxNode> GetChildren() => Members.Select(m => m.Value);

        public void ReplaceExpressions<T>(Func<T, bool> filter, Func<T, Expression> projection) where T : Expression
        {
            foreach (var type in GetTypes())
            {
                type.ReplaceExpressions(filter, projection);
            }
        }
    }
}