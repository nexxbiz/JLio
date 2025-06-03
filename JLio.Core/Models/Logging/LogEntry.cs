using System;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace JLio.Core.Models.Logging;

public class LogEntry
{
    public LogEntry(LogLevel level, string group, string message)
    {
        DateTime = DateTime.UtcNow;
        Level = level;
        Group = group;
        Message = message;
    }

    [JsonProperty("datetime")]
    public DateTime DateTime { get; }

    [JsonProperty("group")]
    public string Group { get; }

    [JsonProperty("level")]
    public LogLevel Level { get; }

    [JsonProperty("message")]
    public string Message { get; }
}