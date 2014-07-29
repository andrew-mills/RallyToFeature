using System;
using NLog;

namespace RallyToFeature
{
    [Serializable]
    public class LoggedException : ApplicationException
    {
        public LoggedException(string message)
            : base(message)
        {
            var logger = LogManager.GetCurrentClassLogger();
            logger.Log(LogLevel.Error, message);
        }

        public LoggedException(string message, params object[] options)
            : base(string.Format(message, options))
        {
            var logger = LogManager.GetCurrentClassLogger();
            logger.Log(LogLevel.Error, String.Format(message, options));
        }

        public LoggedException(string message, Exception innerException)
            : base(message, innerException)
        {

            var logger = LogManager.GetCurrentClassLogger();
            logger.Log(LogLevel.Error, message, innerException);
        }
    }
}