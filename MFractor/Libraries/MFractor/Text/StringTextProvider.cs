using System;
using System.Threading.Tasks;

namespace MFractor.Text
{
    public class StringTextProvider : ITextProvider
    {
        public StringTextProvider(string content)
        {
            Content = content;
        }

        public string Content { get; }

        public string GetText()
        {
            return Content;
        }

        public Task<string> GetTextAsync()
        {
            return Task.FromResult(Content);
        }
    }
}
