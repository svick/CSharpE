using System.Collections.Generic;
using CSharpE.Transform.Internals;

namespace CSharpE.Transform.Transformers
{
    // transformer for a collection of source files
    internal class SourceFileCollectionTransformer<TData> : CollectionTransformer<Syntax.Project, Syntax.SourceFile, TData>
    {
        private readonly ActionInvoker<TData, Syntax.SourceFile> action;
        private readonly TData data;

        private Dictionary<string, CodeTransformer<Syntax.SourceFile>> oldTransfomers;

        public SourceFileCollectionTransformer(
            Syntax.Project parent, ActionInvoker<TData, Syntax.SourceFile> action, TData data)
        {
            this.action = action;
            this.data = data;
        }

        public override void Transform(TransformProject project, IEnumerable<Syntax.SourceFile> sourceFiles)
        {
            var newTransformers = new Dictionary<string, CodeTransformer<Syntax.SourceFile>>();

            foreach (var sourceFile in sourceFiles)
            {
                CodeTransformer<Syntax.SourceFile> fileTransformer = null;

                var path = sourceFile.Path;

                oldTransfomers?.TryGetValue(path, out fileTransformer);

                if (fileTransformer == null)
                    fileTransformer = new CodeTransformer<Syntax.SourceFile>(f => action.Invoke(data, f));

                fileTransformer.Transform(project, sourceFile);

                newTransformers[path] = fileTransformer;
            }

            oldTransfomers = newTransformers;
        }

        public override bool Matches(
            Syntax.Project newParent, ActionInvoker<TData, Syntax.SourceFile> newAction, TData newData) =>
            action.Equals(newAction) && EqualityComparer<TData>.Default.Equals(data, newData);
    }
}