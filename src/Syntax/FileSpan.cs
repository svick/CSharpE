using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace CSharpE.Syntax
{
    public class FileSpan
    {
        private readonly TextSpan span;
        private readonly SyntaxTree file;

        internal FileSpan(TextSpan span, SyntaxTree file)
        {
            this.span = span;
            this.file = file;
        }

        public bool Matches(FileSpan newSpan)
        {
            if (newSpan.span == span && newSpan.file == file)
                return true;

            // PERF: special case for full file spans?

            var changes = newSpan.file.GetChangeRanges(file);

            return span.Compare(newSpan.span, changes) == SpanCompareResult.Matching;
        }
    }
}