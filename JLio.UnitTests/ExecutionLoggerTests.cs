using JLio.Core.Models;
using Microsoft.Extensions.Logging;
using NUnit.Framework;

namespace JLio.UnitTests;

public class ExecutionLoggerTests
{
    [Test]
    public void LogTextFormatsEntries()
    {
        var logger = new ExecutionLogger();
        logger.Log(LogLevel.Information, "group1", "first");
        logger.Log(LogLevel.Warning, "group2", "second");

        var lines = logger.LogText.Trim().Split('\n');
        Assert.AreEqual(2, lines.Length);
        StringAssert.IsMatch(@"\d{2}:\d{2}:\d{2}\.\d{3}\s+Information - first", lines[0]);
        StringAssert.IsMatch(@"\d{2}:\d{2}:\d{2}\.\d{3}\s+Warning - second", lines[1]);
    }
}
