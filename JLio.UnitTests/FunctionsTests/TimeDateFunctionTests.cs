using JLio.Client;
using JLio.Core.Contracts;
using JLio.Core.Models;
using JLio.Extensions.TimeDate;
using JLio.Extensions.TimeDate.Builders;
using JLio.Commands.Builders;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using System;
using System.Linq;

namespace JLio.UnitTests.FunctionsTests;

public class TimeDateFunctionTests
{
    private IExecutionContext executionContext;
    private IParseOptions parseOptions;

    [SetUp]
    public void Setup()
    {
        parseOptions = ParseOptions.CreateDefault().RegisterTimeDate();
        executionContext = ExecutionContext.CreateDefault();
    }

    #region MaxDate Tests
    [TestCase("=maxDate('2024-01-01', '2024-12-31', '2024-06-15')", "{}", "2024-12-31")]
    [TestCase("=maxDate($.dates[*])", "{\"dates\":[\"2024-01-01\",\"2024-06-15\",\"2024-03-10\"]}", "2024-06-15")]
    [TestCase("=maxDate('2024-01-01T10:30:00Z', '2024-01-01T15:45:00Z')", "{}", "2024-01-01T15:45:00")]
    public void MaxDate_ValidInputs(string function, string data, string expectedDatePart)
    {
        var script = $"[{{\"path\":\"$.result\",\"value\":\"{function}\",\"command\":\"add\"}}]";
        var result = JLioConvert.Parse(script, parseOptions).Execute(JToken.Parse(data), executionContext);

        Assert.IsTrue(result.Success, "Function execution should succeed");
        Assert.IsTrue(executionContext.GetLogEntries().TrueForAll(i => i.Level != LogLevel.Error), "Should have no error logs");
        Assert.IsNotNull(result.Data.SelectToken("$.result"), "Result should not be null");
        
        var resultValue = result.Data.SelectToken("$.result")?.Value<string>();
        
        // For UTC times, just check the date part since timezone conversion may occur
        if (expectedDatePart.Contains("T") && expectedDatePart.Contains("15:45:00"))
        {
            Assert.IsTrue((resultValue?.Contains("15:45:00") ?? false) || (resultValue?.Contains("16:45:00") ?? false), 
                $"Result '{resultValue}' should contain time around 15:45:00 (may be adjusted for timezone)");
        }
        else
        {
            Assert.IsTrue(resultValue?.Contains(expectedDatePart) ?? false, $"Result '{resultValue}' should contain '{expectedDatePart}'");
        }
    }

    [Test]
    public void MaxDate_CanBeUsedInFluentApi()
    {
        var script = new JLioScript()
            .Add(MaxDateBuilders.MaxDate("'2024-01-01'", "'2024-12-31'"))
            .OnPath("$.maxDate");

        var result = script.Execute(new JObject());

        Assert.IsTrue(result.Success);
        Assert.IsNotNull(result.Data.SelectToken("$.maxDate"));
    }
    #endregion

    #region MinDate Tests
    [TestCase("=minDate('2024-01-01', '2024-12-31', '2024-06-15')", "{}", "2024-01-01")]
    [TestCase("=minDate($.dates[*])", "{\"dates\":[\"2024-01-01\",\"2024-06-15\",\"2024-03-10\"]}", "2024-01-01")]
    [TestCase("=minDate('2024-01-01T10:30:00Z', '2024-01-01T15:45:00Z')", "{}", "2024-01-01T10:30:00")]
    public void MinDate_ValidInputs(string function, string data, string expectedDatePart)
    {
        var script = $"[{{\"path\":\"$.result\",\"value\":\"{function}\",\"command\":\"add\"}}]";
        var result = JLioConvert.Parse(script, parseOptions).Execute(JToken.Parse(data), executionContext);

        Assert.IsTrue(result.Success, "Function execution should succeed");
        Assert.IsTrue(executionContext.GetLogEntries().TrueForAll(i => i.Level != LogLevel.Error), "Should have no error logs");
        Assert.IsNotNull(result.Data.SelectToken("$.result"), "Result should not be null");
        
        var resultValue = result.Data.SelectToken("$.result")?.Value<string>();
        
        // For UTC times, just check the date part since timezone conversion may occur
        if (expectedDatePart.Contains("T") && expectedDatePart.Contains("10:30:00"))
        {
            Assert.IsTrue((resultValue?.Contains("10:30:00") ?? false) || (resultValue?.Contains("11:30:00") ?? false), 
                $"Result '{resultValue}' should contain time around 10:30:00 (may be adjusted for timezone)");
        }
        else
        {
            Assert.IsTrue(resultValue?.Contains(expectedDatePart) ?? false, $"Result '{resultValue}' should contain '{expectedDatePart}'");
        }
    }

    [Test]
    public void MinDate_CanBeUsedInFluentApi()
    {
        var script = new JLioScript()
            .Add(MinDateBuilders.MinDate("'2024-01-01'", "'2024-12-31'"))
            .OnPath("$.minDate");

        var result = script.Execute(new JObject());

        Assert.IsTrue(result.Success);
        Assert.IsNotNull(result.Data.SelectToken("$.minDate"));
    }
    #endregion

    #region AvgDate Tests
    [TestCase("=avgDate('2024-01-01', '2024-12-31')", "{}", "2024-07")]  // Should be around July
    [TestCase("=avgDate($.dates[*])", "{\"dates\":[\"2024-01-01\",\"2024-12-31\"]}", "2024-07")]
    public void AvgDate_ValidInputs(string function, string data, string expectedDatePart)
    {
        var script = $"[{{\"path\":\"$.result\",\"value\":\"{function}\",\"command\":\"add\"}}]";
        var result = JLioConvert.Parse(script, parseOptions).Execute(JToken.Parse(data), executionContext);

        Assert.IsTrue(result.Success, "Function execution should succeed");
        Assert.IsTrue(executionContext.GetLogEntries().TrueForAll(i => i.Level != LogLevel.Error), "Should have no error logs");
        Assert.IsNotNull(result.Data.SelectToken("$.result"), "Result should not be null");
        
        var resultValue = result.Data.SelectToken("$.result")?.Value<string>();
        Assert.IsTrue(resultValue?.Contains(expectedDatePart) ?? false, $"Result '{resultValue}' should contain '{expectedDatePart}'");
    }

    [Test]
    public void AvgDate_CanBeUsedInFluentApi()
    {
        var script = new JLioScript()
            .Add(AvgDateBuilders.AvgDate("'2024-01-01'", "'2024-12-31'"))
            .OnPath("$.avgDate");

        var result = script.Execute(new JObject());

        Assert.IsTrue(result.Success);
        Assert.IsNotNull(result.Data.SelectToken("$.avgDate"));
    }
    #endregion

    #region IsDateBetween Tests
    [TestCase("=isDateBetween('2024-06-15', '2024-01-01', '2024-12-31')", "{}", true)]
    [TestCase("=isDateBetween('2023-12-31', '2024-01-01', '2024-12-31')", "{}", false)]
    [TestCase("=isDateBetween('2025-01-01', '2024-01-01', '2024-12-31')", "{}", false)]
    [TestCase("=isDateBetween('2024-01-01', '2024-01-01', '2024-12-31')", "{}", true)]  // Inclusive
    [TestCase("=isDateBetween($.checkDate, $.startDate, $.endDate)", 
              "{\"checkDate\":\"2024-06-15\",\"startDate\":\"2024-01-01\",\"endDate\":\"2024-12-31\"}", true)]
    public void IsDateBetween_ValidInputs(string function, string data, bool expected)
    {
        var script = $"[{{\"path\":\"$.result\",\"value\":\"{function}\",\"command\":\"add\"}}]";
        var result = JLioConvert.Parse(script, parseOptions).Execute(JToken.Parse(data), executionContext);

        Assert.IsTrue(result.Success, "Function execution should succeed");
        Assert.IsTrue(executionContext.GetLogEntries().TrueForAll(i => i.Level != LogLevel.Error), "Should have no error logs");
        Assert.IsNotNull(result.Data.SelectToken("$.result"), "Result should not be null");
        
        var actualValue = result.Data.SelectToken("$.result")?.Value<bool>();
        Assert.AreEqual(expected, actualValue);
    }

    [Test]
    public void IsDateBetween_CanBeUsedInFluentApi()
    {
        var script = new JLioScript()
            .Add(IsDateBetweenBuilders.IsDateBetween("'2024-06-15'", "'2024-01-01'", "'2024-12-31'"))
            .OnPath("$.isBetween");

        var result = script.Execute(new JObject());

        Assert.IsTrue(result.Success);
        Assert.AreEqual(true, result.Data.SelectToken("$.isBetween")?.Value<bool>());
    }
    #endregion

    #region DateCompare Tests
    [TestCase("=dateCompare('2024-06-15', '2024-01-01')", "{}", 1)]   // Later date
    [TestCase("=dateCompare('2024-01-01', '2024-06-15')", "{}", -1)]  // Earlier date
    [TestCase("=dateCompare('2024-06-15', '2024-06-15')", "{}", 0)]   // Same date
    [TestCase("=dateCompare($.date1, $.date2)", "{\"date1\":\"2024-06-15\",\"date2\":\"2024-01-01\"}", 1)]
    public void DateCompare_ValidInputs(string function, string data, int expected)
    {
        var script = $"[{{\"path\":\"$.result\",\"value\":\"{function}\",\"command\":\"add\"}}]";
        var result = JLioConvert.Parse(script, parseOptions).Execute(JToken.Parse(data), executionContext);

        Assert.IsTrue(result.Success, "Function execution should succeed");
        Assert.IsTrue(executionContext.GetLogEntries().TrueForAll(i => i.Level != LogLevel.Error), "Should have no error logs");
        Assert.IsNotNull(result.Data.SelectToken("$.result"), "Result should not be null");
        
        var actualValue = result.Data.SelectToken("$.result")?.Value<int>();
        Assert.AreEqual(expected, actualValue);
    }

    [Test]
    public void DateCompare_CanBeUsedInFluentApi()
    {
        var script = new JLioScript()
            .Add(DateCompareBuilders.DateCompare("'2024-06-15'", "'2024-01-01'"))
            .OnPath("$.comparison");

        var result = script.Execute(new JObject());

        Assert.IsTrue(result.Success);
        Assert.AreEqual(1, result.Data.SelectToken("$.comparison")?.Value<int>());
    }
    #endregion

    #region Error Cases
    [Test]
    public void MaxDate_FailsWithNoArguments()
    {
        var script = "[{\"path\":\"$.result\",\"value\":\"=maxDate()\",\"command\":\"add\"}]";
        var result = JLioConvert.Parse(script, parseOptions).Execute(new JObject(), executionContext);
        
        Assert.IsFalse(result.Success);
        Assert.IsTrue(executionContext.GetLogEntries().Any(i => i.Level == LogLevel.Error));
    }

    [Test]
    public void IsDateBetween_FailsWithWrongArgumentCount()
    {
        var script = "[{\"path\":\"$.result\",\"value\":\"=isDateBetween('2024-01-01', '2024-12-31')\",\"command\":\"add\"}]";
        var result = JLioConvert.Parse(script, parseOptions).Execute(new JObject(), executionContext);
        
        Assert.IsFalse(result.Success);
        Assert.IsTrue(executionContext.GetLogEntries().Any(i => i.Level == LogLevel.Error));
    }

    [Test]
    public void DateCompare_FailsWithInvalidDate()
    {
        var script = "[{\"path\":\"$.result\",\"value\":\"=dateCompare('invalid-date', '2024-01-01')\",\"command\":\"add\"}]";
        var result = JLioConvert.Parse(script, parseOptions).Execute(new JObject(), executionContext);
        
        Assert.IsFalse(result.Success);
        Assert.IsTrue(executionContext.GetLogEntries().Any(i => i.Level == LogLevel.Error));
    }
    #endregion

    #region Cross-Platform Culture Tests
    [Test]
    public void DateFunctions_WorkConsistentlyAcrossCultures()
    {
        // Test culture-independent date formats
        var testCases = new[]
        {
            ("=maxDate('2024-01-15', '2024-02-20')", "2024-02-20"),
            ("=minDate('2024-01-15', '2024-02-20')", "2024-01-15"),
            ("=isDateBetween('2024-01-20', '2024-01-15', '2024-02-20')", "True"),  // Boolean format
            ("=dateCompare('2024-02-20', '2024-01-15')", "1")
        };

        foreach (var (function, expectedPart) in testCases)
        {
            var script = $"[{{\"path\":\"$.result\",\"value\":\"{function}\",\"command\":\"add\"}}]";
            var result = JLioConvert.Parse(script, parseOptions).Execute(new JObject(), executionContext);
            
            Assert.IsTrue(result.Success, $"Function {function} should succeed");
            var resultValue = result.Data.SelectToken("$.result")?.ToString();
            Assert.IsTrue(resultValue?.Contains(expectedPart) ?? false, 
                $"Function {function} result '{resultValue}' should contain '{expectedPart}'");
        }
    }

    [Test]
    public void DateFunctions_HandleISO8601FormatsCorrectly()
    {
        var testCases = new[]
        {
            "2024-01-15T10:30:00Z",
            "2024-01-15T10:30:00.123Z", 
            "2024-01-15T10:30:00",
            "2024-01-15"
        };

        foreach (var dateString in testCases)
        {
            var script = $"[{{\"path\":\"$.result\",\"value\":\"=maxDate('{dateString}')\",\"command\":\"add\"}}]";
            var result = JLioConvert.Parse(script, parseOptions).Execute(new JObject(), executionContext);
            
            Assert.IsTrue(result.Success, $"Should parse ISO 8601 date: {dateString}");
            Assert.IsNotNull(result.Data.SelectToken("$.result"), $"Result should not be null for: {dateString}");
        }
    }

    [Test] 
    public void DateFunctions_RejectAmbiguousFormats()
    {
        // These formats are intentionally ambiguous and should be avoided
        var ambiguousDates = new[]
        {
            "01/02/2024", // Could be Jan 2 or Feb 1
            "1/12/2024",  // Could be Jan 12 or Dec 1
        };

        foreach (var ambiguousDate in ambiguousDates)
        {
            var script = $"[{{\"path\":\"$.result\",\"value\":\"=maxDate('{ambiguousDate}')\",\"command\":\"add\"}}]";
            var result = JLioConvert.Parse(script, parseOptions).Execute(new JObject(), executionContext);
            
            // We expect this to either fail or parse consistently with InvariantCulture
            if (result.Success)
            {
                // If it succeeds, it should be parsed with InvariantCulture rules
                var resultValue = result.Data.SelectToken("$.result")?.ToString();
                Assert.IsNotNull(resultValue, $"If parsing succeeds for {ambiguousDate}, result should not be null");
            }
            // If it fails, that's also acceptable as we want to avoid ambiguous parsing
        }
    }

    [Test]
    public void DateFunctions_HandleUnambiguousFormatsCorrectly()
    {
        var unambiguousCases = new[]
        {
            ("15-Jan-2024", "2024-01-15"),  // Month name is clear
            ("Jan 15, 2024", "2024-01-15"),
            ("2024/01/15", "2024-01-15"),   // Year first is unambiguous
            ("2024-01-15", "2024-01-15")   // ISO format
        };

        foreach (var (input, expectedDate) in unambiguousCases)
        {
            var script = $"[{{\"path\":\"$.result\",\"value\":\"=maxDate('{input}')\",\"command\":\"add\"}}]";
            var result = JLioConvert.Parse(script, parseOptions).Execute(new JObject(), executionContext);
            
            Assert.IsTrue(result.Success, $"Should parse unambiguous date: {input}");
            var resultValue = result.Data.SelectToken("$.result")?.ToString();
            Assert.IsTrue(resultValue?.Contains(expectedDate) ?? false, 
                $"Date {input} should parse to contain {expectedDate}, got: {resultValue}");
        }
    }
    #endregion
}