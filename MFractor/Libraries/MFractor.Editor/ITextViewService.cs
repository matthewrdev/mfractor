using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace MFractor.Editor
{
    public interface ITextViewService
    {
        bool HasActiveTextView(string filePath);

        ITextView GetActiveTextView(string filePath);

        IReadOnlyList<ITextView> ActiveTextViews { get; }

        event EventHandler<TextViewEventArgs> OnTextViewOpened;

        event EventHandler<TextViewEventArgs> OnTextViewClosed;
    }
}
