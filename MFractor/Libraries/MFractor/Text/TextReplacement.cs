using System;
using System.Diagnostics;

namespace MFractor.Text
{
    [DebuggerDisplay("{Offset} {Length}")]
    public class TextReplacement : ITextReplacement
    {
        public string Text { get; set; }

        public int Offset { get; set; }

        public int Length { get; set; }

        public string FilePath { get; set; }

        public string Description { get; set; }

        public bool MoveCaretToReplacement { get; set; }
    }

}
