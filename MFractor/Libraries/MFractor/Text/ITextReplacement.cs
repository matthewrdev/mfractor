namespace MFractor.Text
{
    public interface ITextReplacement
    {
        /// <summary>
        /// The new text content.
        /// </summary>
        string Text { get; set; }

        int Offset { get; set; }

        int Length { get; set; }

        string FilePath { get; set; }

        string Description { get; set; }

        bool MoveCaretToReplacement { get; set; }
    }
}