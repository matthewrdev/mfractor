using System;
using MFractor.Fonts.WorkUnits;

namespace MFractor.Views.FontImporter
{
    public class FontImportResultEventArgs : EventArgs
    {
        public FontImportResultEventArgs(FontImportResult fontImportResult)
        {
            FontImportResult = fontImportResult;
        }

        public FontImportResult FontImportResult { get; }
    }
}
