namespace MFractor.Work.WorkUnits
{
    /// <summary>
    /// Inserts the <see cref="Content"/> at the <see cref="Offset"/> into the <see cref="CodeFileWorkUnit.FilePath"/>.
    /// </summary>
    public class InsertTextWorkUnit : CodeFileWorkUnit
    {
        /// <summary>
        /// The text content to insert.
        /// </summary>
        /// <value>The content.</value>
        public string Content { get; }

        /// <summary>
        /// The offset within the file to insert the <see cref="Content"/> at.
        /// </summary>
        /// <value>The offset.</value>
        public int Offset { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="InsertTextWorkUnit"/> class.
        /// </summary>
        /// <param name="content">Content.</param>
        /// <param name="offset">Offset.</param>
        /// <param name="filePath">File path.</param>
        public InsertTextWorkUnit(string content,
                                 int offset,
                                 string filePath)
        {
            Content = content;
            Offset = offset;
            FilePath = filePath;
        }
    }
}
