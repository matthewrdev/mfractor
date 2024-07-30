using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.Text;

namespace MFractor.Text
{
    public interface ILineCollection : IEnumerable<ILine>
    {
        IReadOnlyList<ILine> Lines { get; }

        string Content { get; }

        int LineCount { get; }

        int Length { get; }

        ILine GetLineAtIndex(int index);

        ILine GetLineAtOffset(int offset);

        string GetContent(int offset, int length);

        string GetContent(TextSpan span);

        FileLocation GetLocation(int offset);

        IEnumerable<ILine> GetLines(int offset, int length);

        IEnumerable<ILine> GetLines(TextSpan span);
    }
}