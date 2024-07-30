using System;
using System.ComponentModel.Composition;
using System.Runtime.InteropServices;
using MFractor.Utilities.Clipboards;

namespace MFractor
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(IClipboard))]
    class Clipboard : IClipboard
    {
        public string Text
        {
            get => GetClipboardText();
            set => SetClipboardText(value);
        }

        string GetClipboardText()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return WindowsClipboard.GetText();
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return OsxClipboard.GetText();
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return LinuxClipboard.GetText();
            }

            return string.Empty;
        }

        void SetClipboardText(string value)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                WindowsClipboard.SetText(value ?? string.Empty);
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                OsxClipboard.SetText(value ?? string.Empty);
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                LinuxClipboard.SetText(value ?? string.Empty);
            }
        }
    }
}
