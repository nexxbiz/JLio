using JLio.Client;
using JLio.Core.Contracts;
using JLio.Core.Models;
using JLio.Extensions.Text;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace JLio.UnitTests.FunctionsTests;

public class FetchTests
{
    private IExecutionContext executionContext;
    private ParseOptions parseOptions;

    [SetUp]
    public void Setup()
    {
        parseOptions = ParseOptions.CreateDefault();
        parseOptions.RegisterText(); // Register text functions like concat
        executionContext = ExecutionContext.CreateDefault();
    }

    [Test]
    public void FetchValueByPath()
    {
        var script = "[{'path':'$.result','value':'=fetch($.source)','command':'add'}]".Replace("'","\"");
        var result = JLioConvert.Parse(script, parseOptions)
            .Execute(JObject.Parse("{ 'source': 5 }".Replace("'","\"")), executionContext);
        Assert.IsTrue(result.Success);
        Assert.AreEqual(5, result.Data.SelectToken("$.result")!.Value<int>());
    }

    [Test]
    public void FetchReturnsNullWhenPathMissing()
    {
        var script = "[{'path':'$.result','value':'=fetch($.missing)','command':'add'}]".Replace("'","\"");
        var result = JLioConvert.Parse(script, parseOptions)
            .Execute(new JObject(), executionContext);
        Assert.IsTrue(result.Success);
        Assert.AreEqual(JTokenType.Null, result.Data.SelectToken("$.result")!.Type);
    }

    [Test]
    public void FetchWithNoArgumentsReturnsNull()
    {
        var script = "[{'path':'$.result','value':'=fetch()','command':'add'}]".Replace("'","\"");
        var result = JLioConvert.Parse(script, parseOptions)
            .Execute(new JObject(), executionContext);
        Assert.IsFalse(result.Success);
        Assert.AreEqual(JTokenType.Null, result.Data.SelectToken("$.result")!.Type);
    }

    [Test]
    public void FetchReturnsNullWhenPathPointsToExplicitNull()
    {
        var script = "[{'path':'$.result','value':'=fetch($.nullValue)','command':'add'}]".Replace("'","\"");
        var result = JLioConvert.Parse(script, parseOptions)
            .Execute(JObject.Parse("{ 'nullValue': null }".Replace("'","\"")), executionContext);
        Assert.IsTrue(result.Success);
        Assert.AreEqual(JTokenType.Null, result.Data.SelectToken("$.result")!.Type);
    }

    [Test]
    public void FetchWithDefaultValueReturnsDefaultWhenPathMissing()
    {
        var script = "[{'path':'$.result','value':'=fetch($.missing, 42)','command':'add'}]".Replace("'","\"");
        var result = JLioConvert.Parse(script, parseOptions)
            .Execute(new JObject(), executionContext);
        Assert.IsTrue(result.Success);
        Assert.AreEqual(42, result.Data.SelectToken("$.result")!.Value<int>());
    }

    [Test]
    public void FetchWithDefaultValueReturnsNullWhenPathPointsToExplicitNull()
    {
        var script = "[{'path':'$.result','value':'=fetch($.nullValue, 42)','command':'add'}]".Replace("'","\"");
        var result = JLioConvert.Parse(script, parseOptions)
            .Execute(JObject.Parse("{ 'nullValue': null }".Replace("'","\"")), executionContext);
        Assert.IsTrue(result.Success);
        Assert.AreEqual(JTokenType.Null, result.Data.SelectToken("$.result")!.Type);
    }

    [Test]
    public void FetchWithDefaultValueReturnsActualValueWhenPathExists()
    {
        var script = "[{'path':'$.result','value':'=fetch($.existing, 99)','command':'add'}]".Replace("'","\"");
        var result = JLioConvert.Parse(script, parseOptions)
            .Execute(JObject.Parse("{ 'existing': 42 }".Replace("'","\"")), executionContext);
        Assert.IsTrue(result.Success);
        Assert.AreEqual(42, result.Data.SelectToken("$.result")!.Value<int>());
    }

    [Test]
    public void FetchWithDefaultValueSupportsComplexDefaultValues()
    {
        var script = "[{'path':'$.result','value':'=fetch($.missing, 123)','command':'add'}]".Replace("'","\"");
        var result = JLioConvert.Parse(script, parseOptions)
            .Execute(new JObject(), executionContext);
        Assert.IsTrue(result.Success);
        Assert.AreEqual(123, result.Data.SelectToken("$.result")!.Value<int>());
    }

    [Test]
    public void FetchWithDefaultValueSupportsConcatFunction()
    {
        var function = "=fetch($.missing, concat('Hello', ' ', 'World'))";
        var script = $"[{{\"path\":\"$.result\",\"value\":\"{function}\",\"command\":\"add\"}}]";
        var result = JLioConvert.Parse(script, parseOptions)
            .Execute(new JObject(), executionContext);
        Assert.IsTrue(result.Success);
        Assert.AreEqual("Hello World", result.Data.SelectToken("$.result")!.Value<string>());
    }

    [Test]
    public void FetchWithDefaultValueSupportsNestedFetchFunction()
    {
        var script = "[{'path':'$.result','value':'=fetch($.missing, fetch($.fallback))','command':'add'}]".Replace("'","\"");
        var data = "{ 'fallback': 'backup_value' }".Replace("'","\"");
        var result = JLioConvert.Parse(script, parseOptions)
            .Execute(JObject.Parse(data), executionContext);
        Assert.IsTrue(result.Success);
        Assert.AreEqual("backup_value", result.Data.SelectToken("$.result")!.Value<string>());
    }

    [Test]
    public void FetchWithDefaultValueSupportsConcatWithPaths()
    {
        var function = "=fetch($.missing, concat($.prefix, ' default'))";
        var script = $"[{{\"path\":\"$.result\",\"value\":\"{function}\",\"command\":\"add\"}}]";
        var data = "{ 'prefix': 'User' }".Replace("'","\"");
        var result = JLioConvert.Parse(script, parseOptions)
            .Execute(JObject.Parse(data), executionContext);
        Assert.IsTrue(result.Success);
        Assert.AreEqual("User default", result.Data.SelectToken("$.result")!.Value<string>());
    }

    [Test]
    public void FetchWithDefaultValueDoesNotUseFunctionWhenPathExists()
    {
        var function = "=fetch($.existing, concat('Should', ' not', ' be', ' used'))";
        var script = $"[{{\"path\":\"$.result\",\"value\":\"{function}\",\"command\":\"add\"}}]";
        var data = "{ 'existing': 'actual_value' }".Replace("'","\"");
        var result = JLioConvert.Parse(script, parseOptions)
            .Execute(JObject.Parse(data), executionContext);
        Assert.IsTrue(result.Success);
        Assert.AreEqual("actual_value", result.Data.SelectToken("$.result")!.Value<string>());
    }

    [Test]
    public void FetchWithDefaultValuePreservesNullOverFunction()
    {
        var function = "=fetch($.nullValue, concat('Should', ' not', ' be', ' used'))";
        var script = $"[{{\"path\":\"$.result\",\"value\":\"{function}\",\"command\":\"add\"}}]";
        var data = "{ 'nullValue': null }".Replace("'","\"");
        var result = JLioConvert.Parse(script, parseOptions)
            .Execute(JObject.Parse(data), executionContext);
        Assert.IsTrue(result.Success);
        Assert.AreEqual(JTokenType.Null, result.Data.SelectToken("$.result")!.Type);
    }

    [Test]
    public void FetchWithDefaultValueSupportsComplexNestedFunction()
    {
        var function = "=fetch($.missing, concat(fetch($.company), ' - ', fetch($.department, 'Unknown')))";
        var script = $"[{{\"path\":\"$.result\",\"value\":\"{function}\",\"command\":\"add\"}}]";
        var data = "{ 'company': 'TechCorp' }".Replace("'","\""); // department is missing, not null
        var result = JLioConvert.Parse(script, parseOptions)
            .Execute(JObject.Parse(data), executionContext);
        Assert.IsTrue(result.Success);
        Assert.AreEqual("TechCorp - Unknown", result.Data.SelectToken("$.result")!.Value<string>());
    }

    [Test]
    public void FetchWithDefaultValueSupportsNestedFunctionWithExplicitNull()
    {
        var function = "=fetch($.missing, concat(fetch($.company), ' - ', fetch($.department, 'Unknown')))";
        var script = $"[{{\"path\":\"$.result\",\"value\":\"{function}\",\"command\":\"add\"}}]";
        var data = "{ 'company': 'TechCorp', 'department': null }".Replace("'","\""); // department is explicitly null
        var result = JLioConvert.Parse(script, parseOptions)
            .Execute(JObject.Parse(data), executionContext);
        Assert.IsTrue(result.Success);
        Assert.AreEqual("TechCorp - null", result.Data.SelectToken("$.result")!.Value<string>());
    }

    [Test]
    public void FetchWithDefaultValuePreservesTypesCorrectly()
    {
        // Test integer type preservation
        var scriptInt = "[{'path':'$.result','value':'=fetch($.missing, 42)','command':'add'}]".Replace("'","\"");
        var resultInt = JLioConvert.Parse(scriptInt, parseOptions)
            .Execute(new JObject(), executionContext);
        Assert.IsTrue(resultInt.Success);
        Assert.AreEqual(JTokenType.Integer, resultInt.Data.SelectToken("$.result")!.Type);
        Assert.AreEqual(42, resultInt.Data.SelectToken("$.result")!.Value<int>());

        // Test string type preservation
        var scriptString = "[{\"path\":\"$.result\",\"value\":\"=fetch($.missing, \\\"10\\\")\",\"command\":\"add\"}]";
        var resultString = JLioConvert.Parse(scriptString, parseOptions)
            .Execute(new JObject(), executionContext);
        Assert.IsTrue(resultString.Success);
        Assert.AreEqual(JTokenType.String, resultString.Data.SelectToken("$.result")!.Type);
        Assert.AreEqual("10", resultString.Data.SelectToken("$.result")!.Value<string>());

        // Test array type preservation
        var scriptArray = "[{'path':'$.result','value':'=fetch($.missing, [])','command':'add'}]".Replace("'","\"");
        var resultArray = JLioConvert.Parse(scriptArray, parseOptions)
            .Execute(new JObject(), executionContext);
        Assert.IsTrue(resultArray.Success);
        Assert.AreEqual(JTokenType.Array, resultArray.Data.SelectToken("$.result")!.Type);

        // Test object type preservation
        var scriptObject = "[{'path':'$.result','value':'=fetch($.missing, {})','command':'add'}]".Replace("'","\"");
        var resultObject = JLioConvert.Parse(scriptObject, parseOptions)
            .Execute(new JObject(), executionContext);
        Assert.IsTrue(resultObject.Success);
        Assert.AreEqual(JTokenType.Object, resultObject.Data.SelectToken("$.result")!.Type);

        // Test null type preservation
        var scriptNull = "[{'path':'$.result','value':'=fetch($.missing, null)','command':'add'}]".Replace("'","\"");
        var resultNull = JLioConvert.Parse(scriptNull, parseOptions)
            .Execute(new JObject(), executionContext);
        Assert.IsTrue(resultNull.Success);
        Assert.AreEqual(JTokenType.Null, resultNull.Data.SelectToken("$.result")!.Type);

        // Test boolean type preservation
        var scriptBoolTrue = "[{'path':'$.result','value':'=fetch($.missing, true)','command':'add'}]".Replace("'","\"");
        var resultBoolTrue = JLioConvert.Parse(scriptBoolTrue, parseOptions)
            .Execute(new JObject(), executionContext);
        Assert.IsTrue(resultBoolTrue.Success);
        Assert.AreEqual(JTokenType.Boolean, resultBoolTrue.Data.SelectToken("$.result")!.Type);
        Assert.AreEqual(true, resultBoolTrue.Data.SelectToken("$.result")!.Value<bool>());

        var scriptBoolFalse = "[{'path':'$.result','value':'=fetch($.missing, false)','command':'add'}]".Replace("'","\"");
        var resultBoolFalse = JLioConvert.Parse(scriptBoolFalse, parseOptions)
            .Execute(new JObject(), executionContext);
        Assert.IsTrue(resultBoolFalse.Success);
        Assert.AreEqual(JTokenType.Boolean, resultBoolFalse.Data.SelectToken("$.result")!.Type);
        Assert.AreEqual(false, resultBoolFalse.Data.SelectToken("$.result")!.Value<bool>());

        // Test float type preservation
        var scriptFloat = "[{'path':'$.result','value':'=fetch($.missing, 3.14)','command':'add'}]".Replace("'","\"");
        var resultFloat = JLioConvert.Parse(scriptFloat, parseOptions)
            .Execute(new JObject(), executionContext);
        Assert.IsTrue(resultFloat.Success);
        Assert.AreEqual(JTokenType.Float, resultFloat.Data.SelectToken("$.result")!.Type);
        Assert.AreEqual(3.14, resultFloat.Data.SelectToken("$.result")!.Value<double>(), 0.001);
    }
}
