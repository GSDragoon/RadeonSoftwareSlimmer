using System;
using System.Globalization;

namespace RadeonSoftwareSlimmer.Models
{
    public class LoggingModel
    {
        private const string TIMESTAMP_FORMAT = "yyyy-MM-dd HH:mm:ss.fff";


        public LoggingModel(string message)
        {
            TimeStamp = DateTime.Now;
            Message = message;
        }

        public LoggingModel(Exception exception)
        {
            TimeStamp = DateTime.Now;

            if (exception != null)
            {
                Exception = exception;
                Message = exception.Message;
            }
        }

        public LoggingModel(Exception exception, string message)
        {
            TimeStamp = DateTime.Now;

            if (exception != null)
            {
                Exception = exception;
            }

            Message = message;
        }


        public DateTime TimeStamp { get; }
        public string Message { get; }
        public Exception Exception { get; }


        public override string ToString()
        {
            if (Exception == null)
                return $"{TimeStamp.ToString(TIMESTAMP_FORMAT, CultureInfo.CurrentCulture)}: {Message}";
            else
                return $"{TimeStamp.ToString(TIMESTAMP_FORMAT, CultureInfo.CurrentCulture)}: {Exception}";
        }
    }
}
