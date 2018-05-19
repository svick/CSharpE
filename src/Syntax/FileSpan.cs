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

            int? start = span.Start;
            int? end = span.End;

            // TODO: somehow cache diff?
            var changes = newSpan.file.GetChanges(file);

            foreach (var change in changes)
            {
                start = Adjust(start, change);
                end = Adjust(end, change);
            }

            if (start != null && end != null)
                return newSpan.span == TextSpan.FromBounds(start.Value, end.Value);

            return false;
        }

        private static int? Adjust(int? position, TextChange change)
        {
            if (position == null)
                return null;

            var pos = position.Value;

            int diff = change.NewText.Length - change.Span.Length;

            if (change.Span.End < pos)
                return pos + diff;

            if (change.Span.Start > pos)
                return pos;

            // TODO?
            return null;
        }
    }
}