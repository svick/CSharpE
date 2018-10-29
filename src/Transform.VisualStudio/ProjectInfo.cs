using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using CSharpE.Transform.Execution;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace CSharpE.Transform.VisualStudio
{
    class ProjectInfo
    {
        private static ConcurrentDictionary<ProjectId, ProjectInfo> cache = new ConcurrentDictionary<ProjectId, ProjectInfo>();

        public static ProjectInfo Get(Project project) => cache.GetOrAdd(project.Id, _ => new ProjectInfo());

        private Project oldProject;
        private Project transformedProject;

        private CompilationDiff diff;

        private ProjectTransformer transformer;

        public async Task<(Document, int?)> Adjust(Document document, int position)
        {
            var project = document.Project;

            var adjustedProject = await Adjust(project);

            if (adjustedProject == null)
                return (document, position);

            var adjustedDocument = adjustedProject.GetDocument(document.Id);

            var adjustedPosition = diff.ForTree(document.FilePath).Adjust(position);

            return (adjustedDocument, adjustedPosition);
        }

        private async Task<Project> Adjust(Project project)
        {
            if (oldProject == project)
                return transformedProject;

            var compilation = (CSharpCompilation)await project.GetCompilationAsync();

            // PERF: don't recreate transfomer when references change

            if (transformer == null ||
                !oldProject.MetadataReferences.SequenceEqual(project.MetadataReferences) ||
                !oldProject.ProjectReferences.SequenceEqual(project.ProjectReferences))
            {
                transformer = CreateTransformer(compilation);
            }
            else
            {
                // TODO: what if transformations were added or removed?
                // (because a reference changed even though it compared equal above)

                transformer.ReloadFromCompilation(compilation);
            }

            if (transformer == null)
                return null;

            var result = transformer.Transform(designTime: true);

            var resultProject = oldProject ?? project;

            // TODO: references

            // PERF: use Dictionary?
            var toUpdate = (from document in resultProject.Documents
                           let sourceFile = result.SourceFiles.SingleOrDefault(sf => sf.Path == document.FilePath)
                           where sourceFile != null
                           select (documentId: document.Id, sourceFile)).ToList();

            // update
            foreach (var (documentId, sourceFile) in toUpdate)
            {
                resultProject = resultProject.GetDocument(documentId).WithSyntaxRoot(sourceFile.Tree.GetRoot()).Project;
            }

            // remove
            foreach (var documentId in resultProject.DocumentIds.Where(id => !toUpdate.Any(pair => pair.documentId == id)))
            {
                resultProject = resultProject.RemoveDocument(documentId);
            }

            // add
            foreach (var sourceFile in result.SourceFiles.Where(sf => !toUpdate.Any(pair => pair.sourceFile == sf)))
            {
                resultProject = resultProject.AddDocument(Path.GetFileName(sourceFile.Path), sourceFile.Tree.GetRoot(), filePath: sourceFile.Path).Project;
            }

            oldProject = project;
            transformedProject = resultProject;
            // PERF: lazy?
            diff = new CompilationDiff(await project.GetCompilationAsync(), await resultProject.GetCompilationAsync());

            return resultProject;
        }

        private ProjectTransformer CreateTransformer(CSharpCompilation compilation)
        {
            // TODO: move this code over to ProjectTransformer constructor

            // PERF: caching of transformations and possibly ITransformation

            var iTransformation = compilation.GetTypeByMetadataName(typeof(ITransformation).FullName);

            if (iTransformation == null)
                return null;

            var transformations = new List<ITransformation>();

            // TODO: handle other reference kinds
            foreach (PortableExecutableReference reference in compilation.References)
            {
                var referenceSymbol = (IAssemblySymbol)compilation.GetAssemblyOrModuleSymbol(reference);
                var transformationTypes = GetAllTypesVisitor.FindTypes(
                    referenceSymbol.GlobalNamespace, type => type.TypeKind != TypeKind.Interface && !type.IsAbstract && type.AllInterfaces.Contains(iTransformation));

                if (!transformationTypes.Any())
                    continue;

                Assembly assembly;

                try
                {
                    assembly = Assembly.LoadFrom(reference.FilePath);
                }
                catch (Exception ex)
                {
                    // TODO: produce error
                    return null;
                }


                foreach (var symbol in transformationTypes)
                {
                    var type = assembly.GetType(symbol.GetFullMetadataName());
                    var instance = (ITransformation)Activator.CreateInstance(type);

                    transformations.Add(instance);
                }
            }

            if (!transformations.Any())
                return null;

            return new ProjectTransformer(compilation, transformations);
        }

        // based on https://github.com/dotnet/roslyn/issues/6138#issuecomment-149216303
        private class GetAllTypesVisitor : SymbolVisitor
        {
            public static List<INamedTypeSymbol> FindTypes(INamespaceSymbol ns, Func<INamedTypeSymbol, bool> condition)
            {
                var visitor = new GetAllTypesVisitor(condition);

                ns.Accept(visitor);

                return visitor.types;
            }

            private readonly Func<INamedTypeSymbol, bool> condition;
            private readonly List<INamedTypeSymbol> types = new List<INamedTypeSymbol>();

            private GetAllTypesVisitor(Func<INamedTypeSymbol, bool> condition) => this.condition = condition;

            public override void VisitNamespace(INamespaceSymbol symbol)
            {
                foreach (var s in symbol.GetMembers())
                {
                    s.Accept(this);
                }
            }

            public override void VisitNamedType(INamedTypeSymbol symbol)
            {
                if (condition(symbol))
                    types.Add(symbol);
            }
        }
    }
}
