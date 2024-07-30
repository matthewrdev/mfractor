using System;

namespace MFractor.Text
{
    public interface ITextProviderService
    {
        ITextProvider GetTextProvider(string filePath, TextProviderStrategy strategy = TextProviderStrategy.Cached);
    }
}
