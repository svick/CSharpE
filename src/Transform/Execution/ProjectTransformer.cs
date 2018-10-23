using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharpE.Syntax;
using CSharpE.Syntax.Internals;
using CSharpE.Transform.Transformers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace CSharpE.Transform.Execution
{
    public class ProjectTransformer
    {
        public IList<SourceFile> SourceFiles { get; }
        public IList<LibraryReference> AdditionalReferences { get; }

        private readonly CSharpCompilation compilation;

        private readonly List<TransformationTransformer> transformers;

        public ProjectTransformer(
            IEnumerable<SourceFile> sourceFiles, IEnumerable<Type> additionalReferencesRepresentatives,
            IEnumerable<ITransformation> transformations)
            : this(
                sourceFiles, additionalReferencesRepresentatives.Select(t => new AssemblyReference(t)),
                transformations) { }

        public ProjectTransformer(
            IEnumerable<SourceFile> sourceFiles, IEnumerable<LibraryReference> additionalReferences,
            IEnumerable<ITransformation> transformations)
        {
            SourceFiles = sourceFiles.ToList();
            AdditionalReferences = additionalReferences.ToList();
            transformers = new List<TransformationTransformer>();
            
            foreach (var transformation in transformations)
            {
                AdditionalReferences.AddRange(transformation.AdditionalReferences);
                transformers.Add(new TransformationTransformer(transformation));
            }
        }

        public ProjectTransformer(CSharpCompilation compilation, IEnumerable<ITransformation> transformations)
            : this(
                compilation.SyntaxTrees.Select(tree => new SourceFile(tree)).ToList(),
                // TODO: handle other reference kinds
                compilation.References
                    .Select(reference => new AssemblyReference(((PortableExecutableReference)reference).FilePath))
                    .ToList<LibraryReference>(),
                transformations)
            => this.compilation = compilation;

        public event Action<LogAction> Log;

        public ProjectTransformer Transform(bool designTime = false)
        {
            var transformProject = new TransformProject(
                SourceFiles.Select(f => f.ToSyntaxSourceFile()), AdditionalReferences, compilation, Log);

            foreach (var transformer in transformers)
            {
                transformer.Transform(transformProject, designTime);
            }

            return new ProjectTransformer(
                transformProject.SourceFiles.Select(SourceFile.FromSyntaxSourceFile),
                transformProject.References, Enumerable.Empty<ITransformation>());
        }

        public async Task ReloadSourceFilesAsync()
        {
            foreach (var sourceFile in SourceFiles)
            {
                await sourceFile.ReopenAsync();
            }
        }
    }

    public sealed class LogAction : IEquatable<LogAction>
    {
        public string TargetKind { get; }
        public string TargetName { get; }
        public string Action { get; }

        public LogAction(string targetKind, string targetName, string action)
        {
            TargetKind = targetKind;
            TargetName = targetName;
            Action = action;
        }

        public bool Equals(LogAction other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;

            var ordinalComparer = StringComparer.Ordinal;

            return ordinalComparer.Equals(TargetKind, other.TargetKind) &&
                   ordinalComparer.Equals(TargetName, other.TargetName) && ordinalComparer.Equals(Action, other.Action);
        }

        public override bool Equals(object obj) => Equals(obj as LogAction);

        public override int GetHashCode()
        {
            var ordinalComparer = StringComparer.Ordinal;

            var hashCode = new HashCode();

            hashCode.Add(TargetKind, ordinalComparer);
            hashCode.Add(TargetName, ordinalComparer);
            hashCode.Add(Action, ordinalComparer);

            return hashCode.ToHashCode();
        }

        public static implicit operator LogAction((string TargetKind, string TargetName, string Action) tuple) =>
            new LogAction(tuple.TargetKind, tuple.TargetName, tuple.Action);

        public override string ToString() => (TargetKind, TargetName, Action).ToString();
    }
}