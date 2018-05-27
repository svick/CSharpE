﻿using System;
using System.Collections.Generic;
using System.Linq;
using CSharpE.Syntax;
using CSharpE.Transform.Transformers;

namespace CSharpE.Transform
{
    public class Project
    {
        public IList<SourceFile> SourceFiles { get; }
        public IList<LibraryReference> AdditionalReferences { get; }

        private readonly List<TransformationTransformer> transformers;

        public Project(
            IEnumerable<SourceFile> sourceFiles, IEnumerable<Type> additionalReferencesRepresentatives,
            IEnumerable<ITransformation> transformations)
            : this(
                sourceFiles, additionalReferencesRepresentatives.Select(t => new AssemblyReference(t)),
                transformations) { }

        public Project(
            IEnumerable<SourceFile> sourceFiles, IEnumerable<LibraryReference> additionalReferences,
            IEnumerable<ITransformation> transformations)
        {
            SourceFiles = sourceFiles.ToList();
            AdditionalReferences = additionalReferences.ToList();
            transformers = transformations.Select(t => new TransformationTransformer(t)).ToList();
        }

        public event Action<LogAction> Log;

        public Project Transform()
        {
            var transformProject = new TransformProject(
                SourceFiles.Select(f => f.ToSyntaxSourceFile()), AdditionalReferences, Log);

            foreach (var transformer in transformers)
            {
                transformer.Transform(transformProject);
            }

            return new Project(
                transformProject.SourceFiles.Select(SourceFile.FromSyntaxSourceFile),
                Enumerable.Empty<LibraryReference>(), Enumerable.Empty<ITransformation>());
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