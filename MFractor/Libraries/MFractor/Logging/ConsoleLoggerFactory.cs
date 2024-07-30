namespace MFractor.Logging
{
    public sealed class ConsoleLoggerFactory : ILoggerFactory
    {
        public ILogger Create(string tag)
        {
            return new ConsoleLogger(tag);
        }

        public void Dispose()
        {
        }
    }
}

