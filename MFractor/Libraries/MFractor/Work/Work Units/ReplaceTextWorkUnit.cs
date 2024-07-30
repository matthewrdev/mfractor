using Microsoft.CodeAnalysis.Text;

namespace MFractor.Work.WorkUnits
{
    /// <summary>
    /// A <see cref="IWorkUnit"/> that replaces a section of text in a text file.
    /// <para/>
    /// <see cref="ReplaceTextWorkUnit"/>'s are agreggated, sorted and applied to text files by the <see cref="IWorkEngine"/>.
    /// <para/>
    /// In general, provided that <see cref="Span"/>'s do not overlap, a <see cref="ReplaceTextWorkUnit"/> will be correctly applied regardless of the ordering that are submitted to the <see cref=IWorkEngine"/> in.
    /// </summary>
    public class ReplaceTextWorkUnit : CodeFileWorkUnit
    {
        public ReplaceTextWorkUnit()
        {

        }

        public ReplaceTextWorkUnit(string filePath, string content, TextSpan span)
        {
            FilePath = filePath;
            Text = content;
            Span = span;
        }

        /// <summary>
        /// The new text content to replace at the given <see cref="Span"/>.
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// The span to replace the in the file with the <see cref="Text"/>.
        /// </summary>
        public TextSpan Span
        {
            get;
            set;
        }
    }
}
