using System;
using System.Collections.Generic;
using CSharpE.Syntax;
using CSharpE.Transform.Internals;
using CSharpE.Transform.Transformers;

namespace CSharpE.Transform.Execution
{
    public class ProjectTransformer
    {
        private readonly List<CodeTransformer<TransformProject, Unit>> transformers;

        public ProjectTransformer(IEnumerable<ITransformation> transformations, bool designTime = false)
        {
            transformers = new List<CodeTransformer<TransformProject, Unit>>();
            
            foreach (var transformation in transformations)
            {
                transformers.Add(CreateTransformer(transformation, designTime));
            }
        }

        private static CodeTransformer<TransformProject, Unit> CreateTransformer(ITransformation transformation, bool designTime) =>
            CodeTransformer<TransformProject, Unit>.Create(project =>
            {
                transformation.Process(project, designTime);
                return Unit.Value;
            });

        public event Action<LogAction> Log;

        public Project Transform(Project project)
        {
            var transformProject = new TransformProject(project, Log);

            foreach (var transformer in transformers)
            {
                transformer.Transform(transformProject, transformProject);
            }

            return new Project(transformProject.SourceFiles, transformProject.References);
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