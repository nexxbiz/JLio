using System;
using System.Collections.Generic;
using System.Linq;

namespace JLio.Core.Models.Logging
{
    public class LogEntries : List<LogEntry>
    {
        public DateTime End => this.Max(i => i.DateTime);
        public double ExecutionTimeMilliseconds => Span.TotalMilliseconds;

        private TimeSpan Span => End - Start;
        public DateTime Start => this.Min(i => i.DateTime);
    }
}