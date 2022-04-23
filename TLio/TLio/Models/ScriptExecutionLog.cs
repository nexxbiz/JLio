using System;

namespace TLio.Models
{
    public class ScriptExecutionLog
    {
        public string CommandName { get; set; }
        public DateTimeOffset Timestamp { get; set; }
        public string Message { get; set; }
        public object Payload { get; set; }
    }
}