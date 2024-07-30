namespace MFractor
{
    /// <summary>
    /// Provides access to the platforms clipboard.
    /// </summary>
    public interface IClipboard
    {
        /// <summary>
        /// The clipboards text.
        /// </summary>
        string Text { get; set; }
    }
}