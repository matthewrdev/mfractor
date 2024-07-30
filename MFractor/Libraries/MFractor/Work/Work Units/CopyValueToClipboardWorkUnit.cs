using System;

namespace MFractor.Work.WorkUnits
{
    /// <summary>
    /// An <see cref="IWorkUnit"/> that copies the provided <see cref="Value"/> into the clipboard, showing the <see cref="Message"/> if its not null or empty.
    /// </summary>
    public class CopyValueToClipboardWorkUnit : WorkUnit
    {
        public CopyValueToClipboardWorkUnit()
        {

        }

        public CopyValueToClipboardWorkUnit(string value, string message)
        {
            Value = value;
            Message = message;
        }

        /// <summary>
        /// The value to copy into the clipboard.
        /// </summary>
        /// <value>The value.</value>
        public string Value { get; set; }

        /// <summary>
        /// The message to display when the <see cref="Value"/> is copied into the clipboard.
        /// </summary>
        /// <value>The message.</value>
        public string Message { get; set; }
    }
}
