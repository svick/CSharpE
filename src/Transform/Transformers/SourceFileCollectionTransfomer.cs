using System.Collections.Generic;
using CSharpE.Transform.Internals;

namespace CSharpE.Transform.Transformers
{
    // transformer for a collection of source files
    internal class SourceFileCollectionTransformer<TData, TIntermediate, TResult>
        : CollectionTransformer<Syntax.Project, Syntax.SourceFile, TData, TIntermediate, TResult>
    {
        private Dictionary<string, CodeTransformer<Syntax.SourceFile, TIntermediate>> oldTransfomers;

        public SourceFileCollectionTransformer(
            Syntax.Project parent, ActionInvoker<TData, Syntax.SourceFile, TIntermediate, TResult> action, TData data)
            : base(action, data) { }

        public override TResult Transform(TransformProject project, IEnumerable<Syntax.SourceFile> sourceFiles)
        {
            var newTransformers = new Dictionary<string, CodeTransformer<Syntax.SourceFile, TIntermediate>>();

            foreach (var sourceFile in sourceFiles)
            {
                CodeTransformer<Syntax.SourceFile, TIntermediate> fileTransformer = null;

                var path = sourceFile.Path;

                oldTransfomers?.TryGetValue(path, out fileTransformer);

                if (fileTransformer == null)
                    fileTransformer =
                        CodeTransformer<Syntax.SourceFile, TIntermediate>.Create(f => Action.Invoke(Data, f));

                var intermediate = fileTransformer.Transform(project, sourceFile);
                
                Action.ProvideIntermediate(intermediate);

                newTransformers[path] = fileTransformer;
            }

            oldTransfomers = newTransformers;

            return Action.GetResult();
        }

        public override bool Matches(Syntax.Project newParent,
            ActionInvoker<TData, Syntax.SourceFile, TIntermediate, TResult> newAction, TData newData) =>
            Action.Equals(newAction) && EqualityComparer<TData>.Default.Equals(Data, newData);
    }
}