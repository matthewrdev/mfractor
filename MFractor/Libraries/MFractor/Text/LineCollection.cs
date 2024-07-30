using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis.Text;

namespace MFractor.Text
{
    class LineCollection : ILineCollection
    {
        [DebuggerDisplay("{Content}")]
        class Line : ILine
        {
            public Line(string content, int index, TextSpan span)
            {
                Content = content ?? throw new ArgumentNullException(nameof(content));
                Index = index;
                Span = span;
            }

            public string Content { get; }

            public int Length => Content.Length;

            public int Index { get; }

            public TextSpan Span { get; }
        }

        public LineCollection(string content)
        {
            Content = content ?? throw new ArgumentNullException(nameof(content));
            lines = new Lazy<IReadOnlyList<ILine>>(() =>
            {
                var splitLines = Regex.Split(content, "\r\n|\r|\n");

                var result = new List<ILine>();
                var index = 0;
                var offset = 0;
                foreach (var line in splitLines)
                {
                    var end = offset + line.Length;

                    var lineContent = line;

                    if (end < content.Length)
                    {
                        lineContent += content[end];
                        end += 1;
                    }

                    result.Add(new Line(lineContent, index, TextSpan.FromBounds(offset, end)));

                    offset = end;
                    ++index;
                }

                return result;
            });
        }

        readonly Lazy<IReadOnlyList<ILine>> lines;
        public IReadOnlyList<ILine> Lines => lines.Value;

        public string Content { get; }

        public int Length => Content.Length;

        public int LineCount => Lines.Count;

        public IEnumerator<ILine> GetEnumerator()
        {
            return Lines.GetEnumerator();
        }

        public ILine GetLineAtIndex(int index)
        {
            if (index < 0 || index >= LineCount)
            {
                return null;
            }

            return Lines[index];
        }

        public int GetLineIndexAtOffset(int offset)
        {
            return GetLineAtOffset(offset)?.Index ?? -1;
        }

        public ILine GetLineAtOffset(int offset)
        {
            if (offset < 0 || offset >= Length)
            {
                return null;
            }

            return Lines.FirstOrDefault(l => l.Span.Contains(offset));
        }

        public IEnumerable<ILine> GetLines(int offset, int length)
        {
            return GetLines(TextSpan.FromBounds(offset, offset + length));
        }

        public IEnumerable<ILine> GetLines(TextSpan span)
        {
            var index = GetLineIndexAtOffset(span.Start);

            if (index >= Length)
            {
                return Enumerable.Empty<ILine>();
            }

            index = index < 0 ? 0 : index;

            var lines = new List<ILine>();

            for (; index < LineCount; ++index)
            {
                var line = Lines[index];

                if (line.Span.Start >= span.End)
                {
                    break;
                }

                if (span.IntersectsWith(line.Span))
                {
                    lines.Add(line);
                }

                if (line.Span.End > span.End)
                {
                    break;
                }
            }

            return lines;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Lines.GetEnumerator();
        }

        public FileLocation GetLocation(int offset)
        {
            var line = GetLineAtOffset(offset);

            if (line is null)
            {
                return null;
            }

            var column = offset - line.Span.Start;

            return new FileLocation(line.Index, column);
        }

        public string GetContent(int offset, int length)
        {
            if (offset < 0 || offset >= Length || length < 0)
            {
                return string.Empty;
            }

            if (offset + length >= Length)
            {
                length = Length - offset;
            }

            if (length <= 0)
            {
                return string.Empty;
            }

            return Content.Substring(offset, length);
        }

        public string GetContent(TextSpan span)
        {
            return GetContent(span.Start, span.Length);
        }
    }
}