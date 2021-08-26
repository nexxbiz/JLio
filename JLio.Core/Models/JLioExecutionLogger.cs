using System.Text;
using JLio.Core.Contracts;
using JLio.Core.Models.Logging;
using Microsoft.Extensions.Logging;

namespace JLio.Core.Models
{
    public class JLioExecutionLogger : IJLioExecutionLogger
    {
        public LogEntries LogEntries { get; } = new LogEntries();

        public string LogText
        {
            get
            {
                var stringBuilder = new StringBuilder();
                LogEntries.ForEach(l =>
                    stringBuilder.AppendLine($"{l.DateTime:hh:mm:ss.fff}   {l.Level} - {l.Message}"));
                return stringBuilder.ToString();
            }
        }

        public void Log(LogLevel logLevel, string group, string message)
        {
            LogEntries.Add(new LogEntry(logLevel, group, message));
        }
    }
}