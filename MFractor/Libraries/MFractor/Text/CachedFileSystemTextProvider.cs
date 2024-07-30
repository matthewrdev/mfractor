using System;
using System.IO;
using System.Threading.Tasks;

namespace MFractor.Text
{
    public class CachedFileSystemTextProvider : ITextProvider
    {
        readonly Logging.ILogger log = Logging.Logger.Create();

        readonly Lazy<string> text;

        public CachedFileSystemTextProvider(string filePath)
        {
            text = new Lazy<string>(() =>
           {
               if (string.IsNullOrEmpty(filePath))
               {
                   return string.Empty;
               }

               if (!File.Exists(filePath))
               {
                   log?.Warning("The file " + filePath + " does not exist. Cannot load its text contents");
                   return string.Empty;
               }

               return File.ReadAllText(filePath);
           });
        }

        public string GetText()
        {
            return text.Value;
        }

        public Task<string> GetTextAsync()
        {
            return Task.FromResult(GetText());
        }
    }
}