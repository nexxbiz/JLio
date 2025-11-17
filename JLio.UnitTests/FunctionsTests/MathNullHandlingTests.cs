using JLio.Client;
using JLio.Core.Contracts;
using JLio.Core.Models;
using JLio.Extensions.Math;
using JLio.Extensions.Math.Builders;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using System.Linq;

namespace JLio.UnitTests.FunctionsTests;

/// <summary>
/// Tests for consistent null handling across all math functions.
/// According to requirements:
/// - When a value is NOT FOUND (path doesn't exist): Should result in an ERROR
/// - When a value is FOUND but NULL: Should be treated as 0
/// </summary>
public class MathNullHandlingTests
{
    private IExecutionContext executionContext;
    private IParseOptions parseOptions;

    [SetUp]
    public void Setup()
    {
        parseOptions = ParseOptions.CreateDefault().RegisterMath();
        executionContext = ExecutionContext.CreateDefault();
    }

    #region Sum Function Tests

    [Test]
    public void Sum_WithNullValue_TreatsAsZero()
    {
        // Arrange
        var script = "[{\"path\":\"$.result\",\"value\":\"=sum($.a,$.b)\",\"command\":\"add\"}]";
        var data = "{\"a\":5,\"b\":null}";

        // Act
        var result = JLioConvert.Parse(script, parseOptions).Execute(JToken.Parse(data), executionContext);

        // Assert
        Assert.IsTrue(result.Success, "Sum should succeed when value is found but null");
        Assert.AreEqual(5, result.Data.SelectToken("$.result")?.Value<double>());
    }

    [Test]
    public void Sum_WithNotFoundValue_ReturnsError()
    {
        // Arrange
        var script = "[{\"path\":\"$.result\",\"value\":\"=sum($.a,$.c)\",\"command\":\"add\"}]";
        var data = "{\"a\":5}";

        // Act
        var result = JLioConvert.Parse(script, parseOptions).Execute(JToken.Parse(data), executionContext);

        // Assert
        Assert.IsFalse(result.Success, "Sum should fail when path is not found");
        Assert.IsTrue(executionContext.GetLogEntries().Any(e => 
            e.Level == LogLevel.Error && e.Message.Contains("path not found")));
    }

    [Test]
    public void Sum_WithMultipleNulls_TreatsAllAsZero()
    {
        // Arrange
        var script = "[{\"path\":\"$.result\",\"value\":\"=sum($.a,$.b,$.c)\",\"command\":\"add\"}]";
        var data = "{\"a\":null,\"b\":null,\"c\":10}";

        // Act
        var result = JLioConvert.Parse(script, parseOptions).Execute(JToken.Parse(data), executionContext);

        // Assert
        Assert.IsTrue(result.Success);
        Assert.AreEqual(10, result.Data.SelectToken("$.result")?.Value<double>());
    }

    [Test]
    public void Sum_WithArrayContainingNull_TreatsNullAsZero()
    {
        // Arrange
        var script = "[{\"path\":\"$.result\",\"value\":\"=sum($.numbers[*])\",\"command\":\"add\"}]";
        var data = "{\"numbers\":[5,null,10]}";

        // Act
        var result = JLioConvert.Parse(script, parseOptions).Execute(JToken.Parse(data), executionContext);

        // Assert
        Assert.IsTrue(result.Success);
        Assert.AreEqual(15, result.Data.SelectToken("$.result")?.Value<double>());
    }

    #endregion

    #region Avg Function Tests

    [Test]
    public void Avg_WithNullValue_TreatsAsZero()
    {
        // Arrange
        var script = "[{\"path\":\"$.result\",\"value\":\"=avg($.a,$.b)\",\"command\":\"add\"}]";
        var data = "{\"a\":10,\"b\":null}";

        // Act
        var result = JLioConvert.Parse(script, parseOptions).Execute(JToken.Parse(data), executionContext);

        // Assert
        Assert.IsTrue(result.Success, "Avg should succeed when value is found but null");
        Assert.AreEqual(5, result.Data.SelectToken("$.result")?.Value<double>()); // (10 + 0) / 2 = 5
    }

    [Test]
    public void Avg_WithNotFoundValue_ReturnsError()
    {
        // Arrange
        var script = "[{\"path\":\"$.result\",\"value\":\"=avg($.a,$.c)\",\"command\":\"add\"}]";
        var data = "{\"a\":10}";

        // Act
        var result = JLioConvert.Parse(script, parseOptions).Execute(JToken.Parse(data), executionContext);

        // Assert
        Assert.IsFalse(result.Success, "Avg should fail when path is not found");
        Assert.IsTrue(executionContext.GetLogEntries().Any(e => 
            e.Level == LogLevel.Error && e.Message.Contains("path not found")));
    }

    #endregion

    #region Max Function Tests

    [Test]
    public void Max_WithNullValue_TreatsAsZero()
    {
        // Arrange
        var script = "[{\"path\":\"$.result\",\"value\":\"=max($.a,$.b)\",\"command\":\"add\"}]";
        var data = "{\"a\":5,\"b\":null}";

        // Act
        var result = JLioConvert.Parse(script, parseOptions).Execute(JToken.Parse(data), executionContext);

        // Assert
        Assert.IsTrue(result.Success);
        Assert.AreEqual(5, result.Data.SelectToken("$.result")?.Value<double>()); // max(5, 0) = 5
    }

    [Test]
    public void Max_WithNotFoundValue_ReturnsError()
    {
        // Arrange
        var script = "[{\"path\":\"$.result\",\"value\":\"=max($.a,$.c)\",\"command\":\"add\"}]";
        var data = "{\"a\":5}";

        // Act
        var result = JLioConvert.Parse(script, parseOptions).Execute(JToken.Parse(data), executionContext);

        // Assert
        Assert.IsFalse(result.Success);
        Assert.IsTrue(executionContext.GetLogEntries().Any(e => 
            e.Level == LogLevel.Error && e.Message.Contains("path not found")));
    }

    [Test]
    public void Max_WithNullBeingMaxValue_ReturnsZero()
    {
        // Arrange
        var script = "[{\"path\":\"$.result\",\"value\":\"=max($.a,$.b)\",\"command\":\"add\"}]";
        var data = "{\"a\":-5,\"b\":null}";

        // Act
        var result = JLioConvert.Parse(script, parseOptions).Execute(JToken.Parse(data), executionContext);

        // Assert
        Assert.IsTrue(result.Success);
        Assert.AreEqual(0, result.Data.SelectToken("$.result")?.Value<double>()); // max(-5, 0) = 0
    }

    #endregion

    #region Min Function Tests

    [Test]
    public void Min_WithNullValue_TreatsAsZero()
    {
        // Arrange
        var script = "[{\"path\":\"$.result\",\"value\":\"=min($.a,$.b)\",\"command\":\"add\"}]";
        var data = "{\"a\":5,\"b\":null}";

        // Act
        var result = JLioConvert.Parse(script, parseOptions).Execute(JToken.Parse(data), executionContext);

        // Assert
        Assert.IsTrue(result.Success);
        Assert.AreEqual(0, result.Data.SelectToken("$.result")?.Value<double>()); // min(5, 0) = 0
    }

    [Test]
    public void Min_WithNotFoundValue_ReturnsError()
    {
        // Arrange
        var script = "[{\"path\":\"$.result\",\"value\":\"=min($.a,$.c)\",\"command\":\"add\"}]";
        var data = "{\"a\":5}";

        // Act
        var result = JLioConvert.Parse(script, parseOptions).Execute(JToken.Parse(data), executionContext);

        // Assert
        Assert.IsFalse(result.Success);
        Assert.IsTrue(executionContext.GetLogEntries().Any(e => 
            e.Level == LogLevel.Error && e.Message.Contains("path not found")));
    }

    #endregion

    #region Calculate Function Tests

    [Test]
    public void Calculate_WithNullValue_TreatsAsZero()
    {
        // Arrange
        var script = "[{\"path\":\"$.result\",\"value\":\"=calculate('{{$.a}} + {{$.b}}')\",\"command\":\"add\"}]";
        var data = "{\"a\":5,\"b\":null}";

        // Act
        var result = JLioConvert.Parse(script, parseOptions).Execute(JToken.Parse(data), executionContext);

        // Assert
        Assert.IsTrue(result.Success, "Calculate should succeed when value is found but null");
        Assert.AreEqual(5, result.Data.SelectToken("$.result")?.Value<double>());
    }

    [Test]
    public void Calculate_WithNotFoundValue_ReturnsError()
    {
        // Arrange
        var script = "[{\"path\":\"$.result\",\"value\":\"=calculate('{{$.a}} + {{$.c}}')\",\"command\":\"add\"}]";
        var data = "{\"a\":5}";

        // Act
        var result = JLioConvert.Parse(script, parseOptions).Execute(JToken.Parse(data), executionContext);

        // Assert
        Assert.IsFalse(result.Success, "Calculate should fail when path is not found");
        Assert.IsTrue(executionContext.GetLogEntries().Any(e => 
            e.Level == LogLevel.Error && e.Message.Contains("path not found")));
    }

    [Test]
    public void Calculate_WithMultipleNulls_TreatsAllAsZero()
    {
        // Arrange
        var script = "[{\"path\":\"$.result\",\"value\":\"=calculate('{{$.a}} + {{$.b}} * {{$.c}}')\",\"command\":\"add\"}]";
        var data = "{\"a\":5,\"b\":null,\"c\":10}";

        // Act
        var result = JLioConvert.Parse(script, parseOptions).Execute(JToken.Parse(data), executionContext);

        // Assert
        Assert.IsTrue(result.Success);
        Assert.AreEqual(5, result.Data.SelectToken("$.result")?.Value<double>()); // 5 + 0 * 10 = 5
    }

    #endregion

    #region Other Math Functions Tests

    [Test]
    public void Subtract_WithNullValue_TreatsAsZero()
    {
        // Arrange
        var script = "[{\"path\":\"$.result\",\"value\":\"=subtract($.a,$.b)\",\"command\":\"add\"}]";
        var data = "{\"a\":10,\"b\":null}";

        // Act
        var result = JLioConvert.Parse(script, parseOptions).Execute(JToken.Parse(data), executionContext);

        // Assert
        Assert.IsTrue(result.Success);
        Assert.AreEqual(10, result.Data.SelectToken("$.result")?.Value<double>());
    }

    [Test]
    public void Subtract_WithNotFoundValue_ReturnsError()
    {
        // Arrange
        var script = "[{\"path\":\"$.result\",\"value\":\"=subtract($.a,$.c)\",\"command\":\"add\"}]";
        var data = "{\"a\":10}";

        // Act
        var result = JLioConvert.Parse(script, parseOptions).Execute(JToken.Parse(data), executionContext);

        // Assert
        Assert.IsFalse(result.Success);
    }

    [Test]
    public void Median_WithNullValue_TreatsAsZero()
    {
        // Arrange
        var script = "[{\"path\":\"$.result\",\"value\":\"=median($.a,$.b,$.c)\",\"command\":\"add\"}]";
        var data = "{\"a\":1,\"b\":null,\"c\":3}";

        // Act
        var result = JLioConvert.Parse(script, parseOptions).Execute(JToken.Parse(data), executionContext);

        // Assert
        Assert.IsTrue(result.Success);
        Assert.AreEqual(1, result.Data.SelectToken("$.result")?.Value<double>()); // median(1, 0, 3) = 1
    }

    [Test]
    public void Median_WithNotFoundValue_ReturnsError()
    {
        // Arrange
        var script = "[{\"path\":\"$.result\",\"value\":\"=median($.a,$.c)\",\"command\":\"add\"}]";
        var data = "{\"a\":1}";

        // Act
        var result = JLioConvert.Parse(script, parseOptions).Execute(JToken.Parse(data), executionContext);

        // Assert
        Assert.IsFalse(result.Success);
    }

    [Test]
    public void Abs_WithNullValue_TreatsAsZero()
    {
        // Arrange
        var script = "[{\"path\":\"$.result\",\"value\":\"=abs($.a)\",\"command\":\"add\"}]";
        var data = "{\"a\":null}";

        // Act
        var result = JLioConvert.Parse(script, parseOptions).Execute(JToken.Parse(data), executionContext);

        // Assert
        Assert.IsTrue(result.Success);
        Assert.AreEqual(0, result.Data.SelectToken("$.result")?.Value<double>());
    }

    [Test]
    public void Abs_WithNotFoundValue_ReturnsError()
    {
        // Arrange
        var script = "[{\"path\":\"$.result\",\"value\":\"=abs($.c)\",\"command\":\"add\"}]";
        var data = "{\"a\":1}";

        // Act
        var result = JLioConvert.Parse(script, parseOptions).Execute(JToken.Parse(data), executionContext);

        // Assert
        Assert.IsFalse(result.Success);
    }

    [Test]
    public void Ceiling_WithNullValue_TreatsAsZero()
    {
        // Arrange
        var script = "[{\"path\":\"$.result\",\"value\":\"=ceiling($.a)\",\"command\":\"add\"}]";
        var data = "{\"a\":null}";

        // Act
        var result = JLioConvert.Parse(script, parseOptions).Execute(JToken.Parse(data), executionContext);

        // Assert
        Assert.IsTrue(result.Success);
        Assert.AreEqual(0, result.Data.SelectToken("$.result")?.Value<double>());
    }

    [Test]
    public void Floor_WithNullValue_TreatsAsZero()
    {
        // Arrange
        var script = "[{\"path\":\"$.result\",\"value\":\"=floor($.a)\",\"command\":\"add\"}]";
        var data = "{\"a\":null}";

        // Act
        var result = JLioConvert.Parse(script, parseOptions).Execute(JToken.Parse(data), executionContext);

        // Assert
        Assert.IsTrue(result.Success);
        Assert.AreEqual(0, result.Data.SelectToken("$.result")?.Value<double>());
    }

    [Test]
    public void Round_WithNullValue_TreatsAsZero()
    {
        // Arrange
        var script = "[{\"path\":\"$.result\",\"value\":\"=round($.a)\",\"command\":\"add\"}]";
        var data = "{\"a\":null}";

        // Act
        var result = JLioConvert.Parse(script, parseOptions).Execute(JToken.Parse(data), executionContext);

        // Assert
        Assert.IsTrue(result.Success);
        Assert.AreEqual(0, result.Data.SelectToken("$.result")?.Value<double>());
    }

    [Test]
    public void Sqrt_WithNullValue_TreatsAsZero()
    {
        // Arrange
        var script = "[{\"path\":\"$.result\",\"value\":\"=sqrt($.a)\",\"command\":\"add\"}]";
        var data = "{\"a\":null}";

        // Act
        var result = JLioConvert.Parse(script, parseOptions).Execute(JToken.Parse(data), executionContext);

        // Assert
        Assert.IsTrue(result.Success);
        Assert.AreEqual(0, result.Data.SelectToken("$.result")?.Value<double>());
    }

    [Test]
    public void Pow_WithNullBase_TreatsAsZero()
    {
        // Arrange
        var script = "[{\"path\":\"$.result\",\"value\":\"=pow($.a,$.b)\",\"command\":\"add\"}]";
        var data = "{\"a\":null,\"b\":2}";

        // Act
        var result = JLioConvert.Parse(script, parseOptions).Execute(JToken.Parse(data), executionContext);

        // Assert
        Assert.IsTrue(result.Success);
        Assert.AreEqual(0, result.Data.SelectToken("$.result")?.Value<double>()); // 0^2 = 0
    }

    [Test]
    public void Pow_WithNullExponent_TreatsAsZero()
    {
        // Arrange
        var script = "[{\"path\":\"$.result\",\"value\":\"=pow($.a,$.b)\",\"command\":\"add\"}]";
        var data = "{\"a\":5,\"b\":null}";

        // Act
        var result = JLioConvert.Parse(script, parseOptions).Execute(JToken.Parse(data), executionContext);

        // Assert
        Assert.IsTrue(result.Success);
        Assert.AreEqual(1, result.Data.SelectToken("$.result")?.Value<double>()); // 5^0 = 1
    }

    [Test]
    public void Pow_WithNotFoundValue_ReturnsError()
    {
        // Arrange
        var script = "[{\"path\":\"$.result\",\"value\":\"=pow($.a,$.c)\",\"command\":\"add\"}]";
        var data = "{\"a\":5}";

        // Act
        var result = JLioConvert.Parse(script, parseOptions).Execute(JToken.Parse(data), executionContext);

        // Assert
        Assert.IsFalse(result.Success);
    }

    #endregion

    #region Nested Functions Tests

    [Test]
    public void Calculate_WithMultiplePlaceholders_AndNullValue_TreatsAsZero()
    {
        // Arrange - using calculate with multiple null values
        var script = "[{\"path\":\"$.result\",\"value\":\"=calculate('({{$.a}} + {{$.b}}) + {{$.c}}')\",\"command\":\"add\"}]";
        var data = "{\"a\":5,\"b\":null,\"c\":10}";

        // Act
        var result = JLioConvert.Parse(script, parseOptions).Execute(JToken.Parse(data), executionContext);

        // Assert
        Assert.IsTrue(result.Success);
        Assert.AreEqual(15, result.Data.SelectToken("$.result")?.Value<double>()); // (5 + 0) + 10 = 15
    }

    [Test]
    public void Calculate_WithMultiplePlaceholders_AndNotFoundValue_ReturnsError()
    {
        // Arrange - using calculate with not found value
        var script = "[{\"path\":\"$.result\",\"value\":\"=calculate('({{$.a}} + {{$.x}}) + {{$.c}}')\",\"command\":\"add\"}]";
        var data = "{\"a\":5,\"c\":10}";

        // Act
        var result = JLioConvert.Parse(script, parseOptions).Execute(JToken.Parse(data), executionContext);

        // Assert
        Assert.IsFalse(result.Success, "Calculate should fail when path is not found");
    }

    [Test]
    public void Sum_NestedWithArrays_AndNullValue_TreatsAsZero()
    {
        // Arrange - sum with arrays containing nulls
        var script = "[{\"path\":\"$.result\",\"value\":\"=sum($.numbers[*],$.extras[*])\",\"command\":\"add\"}]";
        var data = "{\"numbers\":[5,null],\"extras\":[10,15]}";

        // Act
        var result = JLioConvert.Parse(script, parseOptions).Execute(JToken.Parse(data), executionContext);

        // Assert
        Assert.IsTrue(result.Success);
        Assert.AreEqual(30, result.Data.SelectToken("$.result")?.Value<double>()); // (5 + 0) + (10 + 15) = 30
    }

    #endregion
}
