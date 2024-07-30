using System;
using System.IO;
using System.Runtime.CompilerServices;
using MFractor.Concurrency;

namespace MFractor.Logging
{
    public sealed class Logger
    {
        public static readonly Logger Instance = new Logger();

        private readonly ConcurrentValue<ILoggerFactory> factory = new ConcurrentValue<ILoggerFactory>(new ConsoleLoggerFactory());

        public ILoggerFactory Factory
        {
            get => factory.Get();
            set => factory.Set(value);
        }

        /// <summary>
        /// Creates a new <see cref="ILogger"/>, capturing the name of the file where is was created as the logger context.
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        public static ILogger Create([CallerFilePath]string tag = "", bool removeFileExtension = true)
        {
            if (tag is null)
            {
                throw new ArgumentNullException(nameof(tag));
            }

            if (Instance == null)
            {
                return null;
            }

            if (Instance.Factory == null)
            {
                return null;
            }

            if (removeFileExtension)
            {
                tag = Path.GetFileNameWithoutExtension(tag);
            }

            return Instance.Factory.Create($"MFractor.{tag}");
        }

        public void Close()
        {
            if (Factory != null)
            {
                Factory.Dispose();
                Factory = null;
            }
        }
    }
}

