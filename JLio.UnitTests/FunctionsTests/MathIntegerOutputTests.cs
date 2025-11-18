using NUnit.Framework;
using JLio.Client;
using JLio.Core.Contracts;
using JLio.Core.Models;
using JLio.Extensions.Math;
using Newtonsoft.Json.Linq;
using System.Globalization;

namespace JLio.UnitTests.FunctionsTests;

public class MathIntegerOutputTests
{
    private IExecutionContext executionContext;
    private IParseOptions parseOptions;

    [SetUp]
    public void Setup()
    {
        parseOptions = ParseOptions.CreateDefault();
        parseOptions.RegisterFunction<Sum>();
        parseOptions.RegisterFunction<Avg>();
        parseOptions.RegisterFunction<Calculate>();
        parseOptions.RegisterFunction<Sqrt>();
        parseOptions.RegisterFunction<Pow>();
        parseOptions.RegisterFunction<Floor>();
        parseOptions.RegisterFunction<Ceiling>();
        parseOptions.RegisterFunction<Round>();
        parseOptions.RegisterFunction<Max>();
        parseOptions.RegisterFunction<Min>();
        
        executionContext = ExecutionContext.CreateDefault();
    }

    [Test]
    public void Sum_WithWholeNumberResult_ReturnsInteger()
    {
        var script = "[{\"path\":\"$.result\",\"value\":\"=sum(1,2,3)\",\"command\":\"add\"}]";
        var result = JLioConvert.Parse(script, parseOptions).Execute(JToken.Parse("{}"), executionContext);
        
        Assert.IsTrue(result.Success);
        var value = result.Data.SelectToken("$.result");
        Assert.AreEqual(JTokenType.Integer, value.Type, "Sum of integers should return Integer type, not Float");
        Assert.AreEqual(6, value.Value<int>());
    }

    [Test]
    public void Avg_WithWholeNumberResult_ReturnsInteger()
    {
        var script = "[{\"path\":\"$.result\",\"value\":\"=avg(2,4,6)\",\"command\":\"add\"}]";
        var result = JLioConvert.Parse(script, parseOptions).Execute(JToken.Parse("{}"), executionContext);
        
        Assert.IsTrue(result.Success);
        var value = result.Data.SelectToken("$.result");
        Assert.AreEqual(JTokenType.Integer, value.Type, "Average that results in whole number should return Integer type");
        Assert.AreEqual(4, value.Value<int>());
    }

    [Test]
    public void Avg_WithDecimalResult_ReturnsFloat()
    {
        var script = "[{\"path\":\"$.result\",\"value\":\"=avg(1,2)\",\"command\":\"add\"}]";
        var result = JLioConvert.Parse(script, parseOptions).Execute(JToken.Parse("{}"), executionContext);
        
        Assert.IsTrue(result.Success);
        var value = result.Data.SelectToken("$.result");
        Assert.AreEqual(JTokenType.Float, value.Type, "Average with decimal result should return Float type");
        Assert.AreEqual(1.5, value.Value<double>(), 0.0001);
    }

    [Test]
    public void Calculate_WithWholeNumberResult_ReturnsInteger()
    {
        var script = "[{\"path\":\"$.result\",\"value\":\"=calculate('5 * 2')\",\"command\":\"add\"}]";
        var result = JLioConvert.Parse(script, parseOptions).Execute(JToken.Parse("{}"), executionContext);
        
        Assert.IsTrue(result.Success);
        var value = result.Data.SelectToken("$.result");
        Assert.AreEqual(JTokenType.Integer, value.Type, "Calculate with integer result should return Integer type");
        Assert.AreEqual(10, value.Value<int>());
    }

    [Test]
    public void Sqrt_WithWholeNumberResult_ReturnsInteger()
    {
        var script = "[{\"path\":\"$.result\",\"value\":\"=sqrt(16)\",\"command\":\"add\"}]";
        var result = JLioConvert.Parse(script, parseOptions).Execute(JToken.Parse("{}"), executionContext);
        
        Assert.IsTrue(result.Success);
        var value = result.Data.SelectToken("$.result");
        Assert.AreEqual(JTokenType.Integer, value.Type, "Sqrt of perfect square should return Integer type");
        Assert.AreEqual(4, value.Value<int>());
    }

    [Test]
    public void Sqrt_WithDecimalResult_ReturnsFloat()
    {
        var script = "[{\"path\":\"$.result\",\"value\":\"=sqrt(2)\",\"command\":\"add\"}]";
        var result = JLioConvert.Parse(script, parseOptions).Execute(JToken.Parse("{}"), executionContext);
        
        Assert.IsTrue(result.Success);
        var value = result.Data.SelectToken("$.result");
        Assert.AreEqual(JTokenType.Float, value.Type, "Sqrt with decimal result should return Float type");
        Assert.AreEqual(1.414, value.Value<double>(), 0.001);
    }

    [Test]
    public void Pow_WithWholeNumberResult_ReturnsInteger()
    {
        var script = "[{\"path\":\"$.result\",\"value\":\"=pow(2,3)\",\"command\":\"add\"}]";
        var result = JLioConvert.Parse(script, parseOptions).Execute(JToken.Parse("{}"), executionContext);
        
        Assert.IsTrue(result.Success);
        var value = result.Data.SelectToken("$.result");
        Assert.AreEqual(JTokenType.Integer, value.Type, "Pow with integer result should return Integer type");
        Assert.AreEqual(8, value.Value<int>());
    }

    [Test]
    public void Floor_ReturnsInteger()
    {
        var script = "[{\"path\":\"$.result\",\"value\":\"=floor(5.7)\",\"command\":\"add\"}]";
        var result = JLioConvert.Parse(script, parseOptions).Execute(JToken.Parse("{}"), executionContext);
        
        Assert.IsTrue(result.Success);
        var value = result.Data.SelectToken("$.result");
        Assert.AreEqual(JTokenType.Integer, value.Type, "Floor should always return Integer type");
        Assert.AreEqual(5, value.Value<int>());
    }

    [Test]
    public void Ceiling_ReturnsInteger()
    {
        var script = "[{\"path\":\"$.result\",\"value\":\"=ceiling(5.3)\",\"command\":\"add\"}]";
        var result = JLioConvert.Parse(script, parseOptions).Execute(JToken.Parse("{}"), executionContext);
        
        Assert.IsTrue(result.Success);
        var value = result.Data.SelectToken("$.result");
        Assert.AreEqual(JTokenType.Integer, value.Type, "Ceiling should always return Integer type");
        Assert.AreEqual(6, value.Value<int>());
    }

    [Test]
    public void Round_WithNoDecimals_ReturnsInteger()
    {
        var script = "[{\"path\":\"$.result\",\"value\":\"=round(5.7)\",\"command\":\"add\"}]";
        var result = JLioConvert.Parse(script, parseOptions).Execute(JToken.Parse("{}"), executionContext);
        
        Assert.IsTrue(result.Success);
        var value = result.Data.SelectToken("$.result");
        Assert.AreEqual(JTokenType.Integer, value.Type, "Round with no decimal places should return Integer type");
        Assert.AreEqual(6, value.Value<int>());
    }
    
    [Test]
    public void Max_WithWholeNumberResult_ReturnsInteger()
    {
        var script = "[{\"path\":\"$.result\",\"value\":\"=max(1,5,3)\",\"command\":\"add\"}]";
        var result = JLioConvert.Parse(script, parseOptions).Execute(JToken.Parse("{}"), executionContext);
        
        Assert.IsTrue(result.Success);
        var value = result.Data.SelectToken("$.result");
        Assert.AreEqual(JTokenType.Integer, value.Type, "Max with integer inputs should return Integer type");
        Assert.AreEqual(5, value.Value<int>());
    }
    
    [Test]
    public void Min_WithWholeNumberResult_ReturnsInteger()
    {
        var script = "[{\"path\":\"$.result\",\"value\":\"=min(1,5,3)\",\"command\":\"add\"}]";
        var result = JLioConvert.Parse(script, parseOptions).Execute(JToken.Parse("{}"), executionContext);
        
        Assert.IsTrue(result.Success);
        var value = result.Data.SelectToken("$.result");
        Assert.AreEqual(JTokenType.Integer, value.Type, "Min with integer inputs should return Integer type");
        Assert.AreEqual(1, value.Value<int>());
    }
}
