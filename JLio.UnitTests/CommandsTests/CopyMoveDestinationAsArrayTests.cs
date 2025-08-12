using JLio.Commands;
using JLio.Core.Contracts;
using JLio.Core.Models;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace JLio.UnitTests.CommandsTests;

public class CopyMoveDestinationAsArrayTests
{
    private JToken data;
    private IExecutionContext executeOptions;

    [SetUp]
    public void Setup()
    {
        executeOptions = ExecutionContext.CreateDefault();
        data = JToken.Parse("{ \"val\": 1, \"arr\": [1], \"obj\": {\"val\": 1} }");
    }

    [Test]
    public void CopyToScalarDestinationAsArray_AppendsOldAndNew()
    {
        var result = new Copy("$.val", "$.val", true).Execute(data, executeOptions);
        Assert.IsTrue(result.Success);
        var arr = data["val"] as JArray;
        Assert.IsNotNull(arr);
        Assert.AreEqual(2, arr.Count);
        Assert.AreEqual(1, arr[0].Value<int>());
        Assert.AreEqual(1, arr[1].Value<int>());
    }

    [Test]
    public void CopyToExistingArrayDestinationAsArray_Appends()
    {
        var result = new Copy("$.val", "$.arr", true).Execute(data, executeOptions);
        Assert.IsTrue(result.Success);
        var arr = data["arr"] as JArray;
        Assert.IsNotNull(arr);
        Assert.AreEqual(2, arr.Count);
        Assert.AreEqual(1, arr[0].Value<int>());
        Assert.AreEqual(1, arr[1].Value<int>());
    }

    [Test]
    public void MoveToScalarDestinationAsArray_AppendsOldAndNewAndRemovesSource()
    {
        var result = new Move("$.val", "$.val", true).Execute(data, executeOptions);
        Assert.IsTrue(result.Success);
        var arr = data["val"] as JArray;
        Assert.IsNotNull(arr);
        Assert.AreEqual(2, arr.Count);
        Assert.AreEqual(1, arr[0].Value<int>());
        Assert.AreEqual(1, arr[1].Value<int>());
    }
}
