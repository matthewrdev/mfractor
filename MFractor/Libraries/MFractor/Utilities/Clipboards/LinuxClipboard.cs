﻿using System;
using System.IO;

namespace MFractor.Utilities.Clipboards
{
    /// <summary>
    /// Accesses the Linux platforms clipboard.
    /// <para/>
    /// Credit: https://github.com/SimonCropp/TextCopy
    /// </summary>
    static class LinuxClipboard
    {
        public static void SetText(string text)
        {
            var tempFileName = Path.GetTempFileName();
            File.WriteAllText(tempFileName, text);
            try
            {
                BashRunner.Run($"cat {tempFileName} | xclip");
            }
            finally
            {
                File.Delete(tempFileName);
            }
        }

        public static string GetText()
        {
            var tempFileName = Path.GetTempFileName();
            try
            {
                BashRunner.Run($"xclip -o > {tempFileName}");
                return File.ReadAllText(tempFileName);
            }
            finally
            {
                File.Delete(tempFileName);
            }
        }
    }
}
