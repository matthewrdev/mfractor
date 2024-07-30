using System;

namespace MFractor.Logging
{
	public interface ILoggerFactory : IDisposable
	{
		ILogger Create(string tag);
	}
}

