using System.Collections.Generic;
using System.Linq;
using JLio.Commands.Advanced;
using JLio.Commands.Advanced.Builders;
using JLio.Core.Contracts;
using JLio.Core.Models;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace JLio.UnitTests.CommandsTests;

public class DistinctTests
{
    private IExecutionContext executeOptions;

    [SetUp]
    public void Setup()
    {
        executeOptions = ExecutionContext.CreateDefault();
    }

    [Test]
    public void DistinctSimpleArray()
    {
        var data = JObject.Parse("{\"arr\": [1,2,3,4,2] }");
        var result = new Distinct("$.arr").Execute(data, executeOptions);

        Assert.IsTrue(result.Success);
        Assert.IsTrue(JToken.DeepEquals(JArray.Parse("[1,2,3,4]"), data.SelectToken("$.arr")));
    }

    [Test]
    public void DistinctComplexArrayWithKeys()
    {
        var data = JObject.Parse("{\"arr\":[{\"id\":1,\"value\":\"a\"},{\"id\":2,\"value\":\"b\"},{\"id\":1,\"value\":\"c\"}]}");
        var command = new Distinct("$.arr") { KeyPaths = new List<string> { "id" } };
        command.Execute(data, executeOptions);

        var expected = JArray.Parse("[{\"id\":1,\"value\":\"c\"},{\"id\":2,\"value\":\"b\"}]");
        Assert.IsTrue(JToken.DeepEquals(expected, data.SelectToken("$.arr")));
    }

    [Test]
    public void CanUseFluentApi()
    {
        var script = new JLioScript()
            .Distinct("$.arr")
            .Distinct("$.arr2", new List<string> { "id" });
        var result = script.Execute(new JObject
        {
            ["arr"] = new JArray(1,2,2),
            ["arr2"] = new JArray(new JObject { ["id"] = 1 }, new JObject { ["id"] = 1 })
        });

        Assert.IsNotNull(result);
        Assert.IsTrue(result.Success);
        Assert.AreEqual(2, result.Data["arr"].Count());
        Assert.AreEqual(1, result.Data["arr2"].Count());
    }
}

