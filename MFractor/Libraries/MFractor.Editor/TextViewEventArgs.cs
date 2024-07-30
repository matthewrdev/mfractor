using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace MFractor.Editor
{
    public class TextViewEventArgs : EventArgs
    {
        public TextViewEventArgs(string filePath, ITextView textView)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                throw new ArgumentException("message", nameof(filePath));
            }

            FilePath = filePath;
            TextView = textView ?? throw new ArgumentNullException(nameof(textView));
        }

        public string FilePath { get; }

        public ITextView TextView { get; }
    }
}
