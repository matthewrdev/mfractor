using System;
namespace MFractor.Views.Picker
{
    public class PickerSelectionEventArgs : EventArgs
    {
        public PickerSelectionEventArgs(string label, object item)
        {
            Label = label;
            Item = item;
        }

        public string Label { get; }

        public object Item { get; }
    }
}
