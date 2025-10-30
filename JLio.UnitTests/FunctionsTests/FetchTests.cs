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
    public void FetchWithDefaultValueSupportsStringLiterals()
    {
        var function = "=fetch($.missing, 'hello world')";
        var script = $"[{{\"path\":\"$.result\",\"value\":\"{function}\",\"command\":\"add\"}}]";
        var result = JLioConvert.Parse(script, parseOptions)
            .Execute(new JObject(), executionContext);
        Assert.IsTrue(result.Success);
        Assert.AreEqual("hello world", result.Data.SelectToken("$.result")!.Value<string>());
    }

    [Test]
    public void FetchWithDefaultValueSupportsArrayLiterals()
    {
        var function = "=fetch($.missing, [1, 2, 3])";
        var script = $"[{{\"path\":\"$.result\",\"value\":\"{function}\",\"command\":\"add\"}}]";
        var result = JLioConvert.Parse(script, parseOptions)
            .Execute(new JObject(), executionContext);
        Assert.IsTrue(result.Success);
        var resultArray = result.Data.SelectToken("$.result") as JArray;
        Assert.IsNotNull(resultArray);
        Assert.AreEqual(3, resultArray.Count);
        Assert.AreEqual(1, resultArray[0]!.Value<int>());
        Assert.AreEqual(2, resultArray[1]!.Value<int>());
        Assert.AreEqual(3, resultArray[2]!.Value<int>());
    }

    [Test]
    public void FetchWithDefaultValueSupportsObjectLiterals()
    {
        var function = "=fetch($.missing, { 'demo': 42, 'name': 'test' })";
        var script = $"[{{\"path\":\"$.result\",\"value\":\"{function}\",\"command\":\"add\"}}]";
        var result = JLioConvert.Parse(script, parseOptions)
            .Execute(new JObject(), executionContext);
        Assert.IsTrue(result.Success);
        var resultObject = result.Data.SelectToken("$.result") as JObject;
        Assert.IsNotNull(resultObject);
        Assert.AreEqual(42, resultObject.SelectToken("$.demo")!.Value<int>());
        Assert.AreEqual("test", resultObject.SelectToken("$.name")!.Value<string>());
    }

    [Test]
    public void FetchWithDefaultValueSupportsComplexNestedStructures()
    {
        var function = "=fetch($.missing, { 'users': ['alice', 'bob'], 'count': 2 })";
        var script = $"[{{\"path\":\"$.result\",\"value\":\"{function}\",\"command\":\"add\"}}]";
        var result = JLioConvert.Parse(script, parseOptions)
            .Execute(new JObject(), executionContext);
        Assert.IsTrue(result.Success);
        var resultObject = result.Data.SelectToken("$.result") as JObject;
        Assert.IsNotNull(resultObject);
        Assert.AreEqual(2, resultObject.SelectToken("$.count")!.Value<int>());
        var usersArray = resultObject.SelectToken("$.users") as JArray;
        Assert.IsNotNull(usersArray);
        Assert.AreEqual("alice", usersArray[0]!.Value<string>());
        Assert.AreEqual("bob", usersArray[1]!.Value<string>());
    }

    [Test]
    public void FetchWithDefaultValuePreservesTypeOfSimpleValues()
    {
        // Test boolean
        var boolFunction = "=fetch($.missing, true)";
        var boolScript = $"[{{\"path\":\"$.result\",\"value\":\"{boolFunction}\",\"command\":\"add\"}}]";
        var boolResult = JLioConvert.Parse(boolScript, parseOptions)
            .Execute(new JObject(), executionContext);
        Assert.IsTrue(boolResult.Success);
        Assert.AreEqual(true, boolResult.Data.SelectToken("$.result")!.Value<bool>());

        // Test null
        var nullFunction = "=fetch($.missing, null)";
        var nullScript = $"[{{\"path\":\"$.result\",\"value\":\"{nullFunction}\",\"command\":\"add\"}}]";
        var nullResult = JLioConvert.Parse(nullScript, parseOptions)
            .Execute(new JObject(), executionContext);
        Assert.IsTrue(nullResult.Success);
        Assert.AreEqual(JTokenType.Null, nullResult.Data.SelectToken("$.result")!.Type);

        // Test number
        var numberFunction = "=fetch($.missing, 42.5)";
        var numberScript = $"[{{\"path\":\"$.result\",\"value\":\"{numberFunction}\",\"command\":\"add\"}}]";
        var numberResult = JLioConvert.Parse(numberScript, parseOptions)
            .Execute(new JObject(), executionContext);
        Assert.IsTrue(numberResult.Success);
        Assert.AreEqual(42.5, numberResult.Data.SelectToken("$.result")!.Value<double>());
    }
}
