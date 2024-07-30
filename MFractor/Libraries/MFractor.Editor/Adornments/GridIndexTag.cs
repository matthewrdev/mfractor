using System;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;

namespace MFractor.Editor.Adornments
{
    public class GridIndexTag : ITag
    {
        public GridIndexTag(int index, string sampleCode)
        {
            Index = index;
            SampleCode = sampleCode;
        }

        public int Index { get; }

        public string SampleCode { get; }

        public override string ToString()
        {
            return Index.ToString();
        }
    }
}