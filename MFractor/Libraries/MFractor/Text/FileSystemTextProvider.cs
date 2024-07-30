using System;
using System.IO;
using System.Threading.Tasks;

namespace MFractor.Text
{
    public class FileSystemTextProvider : ITextProvider
    {
        public FileSystemTextProvider(string filePath)
        {
            FilePath = filePath;
        }

        public string FilePath { get; }

        public string GetText()
        {
            if (string.IsNullOrEmpty(FilePath))
            {
                return string.Empty;
            }

            if (!File.Exists(FilePath))
            {
                return string.Empty;
            }

            return File.ReadAllText(FilePath);
        }

        public Task<string> GetTextAsync()
        {
            return Task.FromResult(GetText());
        }
    }
}