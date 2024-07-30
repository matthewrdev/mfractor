namespace MFractor.Text
{
    public abstract class TextProviderService : ITextProviderService
    {
        protected abstract bool IsActiveFile(string filePath);

        protected abstract ITextProvider GetActiveFileTextProvider(string filePath);

        public ITextProvider GetTextProvider(string filePath, TextProviderStrategy strategy = TextProviderStrategy.Cached)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                return null;
            }

            if (IsActiveFile(filePath))
            {
                return GetActiveFileTextProvider(filePath);
            }

            switch (strategy)
            {
                case TextProviderStrategy.Cached:
                    return new CachedFileSystemTextProvider(filePath);
                case TextProviderStrategy.Default:
                default:
                    return new FileSystemTextProvider(filePath);
            }
        }
    }
}