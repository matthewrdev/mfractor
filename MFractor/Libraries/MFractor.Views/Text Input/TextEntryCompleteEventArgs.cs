using System;

namespace MFractor.Views.TextInput
{
    public class TextEntryCompleteEventArgs : EventArgs
    {
        public readonly string Value;

        public TextEntryCompleteEventArgs(string value)
        {
            Value = value;
        }
    }
}

