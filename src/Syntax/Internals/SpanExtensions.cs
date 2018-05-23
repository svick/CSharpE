using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.Text;
using static CSharpE.Syntax.Internals.SpanCompareResult;

namespace CSharpE.Syntax.Internals
{
    internal enum SpanCompareResult
    {
        Unknown,
        Matching,
        NewFirst,
        OldFirst
    }

    internal static class SpanExtensions
    {
        public static SpanCompareResult Compare(
            this TextSpan oldSpan, TextSpan newSpan, IReadOnlyList<TextChangeRange> changes)
        {
            if (!changes.Any() && oldSpan == newSpan)
                return Matching;

            int? start = oldSpan.Start;
            int? end = oldSpan.End;

            foreach (var change in changes)
            {
                start = Adjust(start, change);
                end = Adjust(end, change);
            }

            if (start != null)
            {
                if (end != null && newSpan == TextSpan.FromBounds(start.Value, end.Value))
                    return Matching;

                if (start < newSpan.Start)
                    return OldFirst;

                if (end < newSpan.End)
                    return NewFirst;
            }

            return Unknown;
        }

        private static int? Adjust(int? position, TextChangeRange change)
        {
            if (position == null)
                return null;

            var pos = position.Value;

            int diff = change.NewLength - change.Span.Length;

            if (change.Span.End < pos)
                return pos + diff;

            if (change.Span.Start > pos)
                return pos;

            // TODO?
            return null;
        }
    }
}