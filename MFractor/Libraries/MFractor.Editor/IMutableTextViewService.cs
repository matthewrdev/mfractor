using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace MFractor.Editor
{
    interface IMutableTextViewService : ITextViewService
    {
        void NotifyOpened(string filePath);
        void NotifyOpened(string filePath, ITextView textView);

        void NotifyClosed(string filePath);
        void NotifyClosed(string filePath, ITextView textView);

        void BindTextView(string filePath, string projectGuid, ITextView textView);
    }
}
