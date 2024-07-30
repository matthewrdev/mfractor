using System;
using System.Windows.Controls;
using MFractor.IOC;
using MFractor.VS.Windows.Utilities;

namespace MFractor.VS.Windows.Adornments
{
    public class GridIndexAdornment : Label
    {
        readonly Logging.ILogger log = Logging.Logger.Create();

        public int Index { get; private set; }

        public string SampleCode { get; private set; }

        public GridIndexAdornment() : base()
        {
            Build();
        }

        public GridIndexAdornment(int index, string sampleCode) : base()
        {
            Build();
            SetData(index, sampleCode);
        }

        void Build()
        {
            //
            // Event Handlers
            MouseLeftButtonDown += OnMouseLeftButtonDown;

            //
            // Setup UI
            FontFamily = new System.Windows.Media.FontFamily(UIHelpers.GetTextEditorFontFamilyName());
            FontSize = Math.Floor(UIHelpers.GetTextEditorFontSize() * 0.9);
            Foreground = UIHelpers.GetTextEditorPlainTextColor()
                .ToMediaColor()
                .ToBrush();
        }

        void OnMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try
            {
                if (!string.IsNullOrEmpty(SampleCode))
                {
                    Resolver.Resolve<IClipboard>().Text = SampleCode;
                    Resolver.Resolve<IDialogsService>().StatusBarMessage($"Copied {SampleCode} to clipboard");
                }
            }
            catch (Exception ex)
            {
                log?.Exception(ex);
            }
        }

        public void SetData(int index, string sampleCode)
        {
            Index = index;
            SampleCode = sampleCode;

            var newContent = index.ToString();
            if (Content == null || (Content is string content && content != newContent))
            {
                Content = newContent;
            }
        }

    }
}
