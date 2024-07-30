using System;

namespace MFractor.Logging
{
	public interface ILogger
	{
		string Tag { get; }

		void Log(string tag, string message, LogLevel logLevel);

		void Error(string message);
		void Warning(string message);
		void Info(string message);
		void Debug(string message);
		void Verbose(string message);

		void Exception(Exception ex);
	}
}
