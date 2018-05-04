using System;

namespace CSharpE.Syntax.Internals
{
    // TODO: probably not necessary anymore
    public class WrapperContext
    {
        private readonly SourceFile sourceFile;

        internal WrapperContext(SourceFile sourceFile) =>
            this.sourceFile = sourceFile ?? throw new ArgumentNullException(nameof(sourceFile));

        internal void EnsureUsingNamespace(string ns) => sourceFile.EnsureUsingNamespace(ns);
    }
}