using System.Collections.Generic;
using CSharpE.Syntax;
using CSharpE.Transform.Internals;

namespace CSharpE.Transform.Transformers
{
    // transformer for a collection of source files
    internal sealed class SourceFileCollectionTransformer<TData, TIntermediate, TResult>
        : CollectionTransformer<Project, SourceFile, TData, TIntermediate, TResult>
    {
        private Dictionary<string, CodeTransformer<SourceFile, TIntermediate>> oldTransformers;

        public SourceFileCollectionTransformer(
            Project parent, ActionInvoker<TData, SourceFile, TIntermediate, TResult> action, TData data)
            : base(action, data) { }

        public override TResult Transform(TransformProject project, IEnumerable<SourceFile> sourceFiles)
        {
            var newTransformers = new Dictionary<string, CodeTransformer<SourceFile, TIntermediate>>();

            foreach (var sourceFile in sourceFiles)
            {
                CodeTransformer<SourceFile, TIntermediate> fileTransformer = null;

                var path = sourceFile.Path;

                oldTransformers?.TryGetValue(path, out fileTransformer);

                if (fileTransformer == null)
                    fileTransformer = CodeTransformer<SourceFile, TIntermediate>.Create(InvokeAndCheck);

                var intermediate = fileTransformer.Transform(project, sourceFile);
                
                Action.ProvideIntermediate(intermediate);

                newTransformers[path] = fileTransformer;
            }

            oldTransformers = newTransformers;

            return Action.GetResult();
        }

        public override bool Matches(Project newParent,
            ActionInvoker<TData, SourceFile, TIntermediate, TResult> newAction, TData newData,
            bool newLimitedComparison) =>
            Action.Equals(newAction) && EqualityComparer<TData>.Default.Equals(Data, newData);
    }
}