using System;

namespace MFractor.Views
{
    /// <summary>
    /// A wrapper interface for an IDE specific text editor representation.
    /// </summary>
    public interface ITextEditor : IDisposable
    {
        /// <summary>
        /// The <see cref="Xwt.Widget"/> that represents the text editor,
        /// </summary>
        Xwt.Widget Widget { get; }

        /// <summary>
        /// The current text buffer of the editor.
        /// </summary>
        string Text { get; set; }

        /// <summary>
        /// The current mimetype of the editor.
        /// </summary>
        string MimeType { get; set; }

        /// <summary>
        /// If this editor is readonly, aka, does it allow user input?
        /// </summary>
        bool IsReadOnly { get; set; }

        /// <summary>
        /// Occurs when the current text selection is changed.
        /// </summary>
        event EventHandler<EventArgs> SelectionChanged;

        /// <summary>
        /// Clears the current text selection.
        /// </summary>
        void ClearSelection();
    }
}
