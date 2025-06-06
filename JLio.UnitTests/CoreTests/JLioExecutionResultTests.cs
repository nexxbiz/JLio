using JLio.Core.Models;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace JLio.UnitTests.CoreTests;

public class JLioExecutionResultTests
{
    [Test]
    public void ConstructorSetsProperties()
    {
        var token = JToken.Parse("{\"a\":1}");
        var result = new JLioExecutionResult(true, token);
        Assert.IsTrue(result.Success);
        Assert.AreSame(token, result.Data);
    }

    [Test]
    public void FailedCreatesFailedResult()
    {
        var token = new JObject();
        var result = JLioExecutionResult.Failed(token);
        Assert.IsFalse(result.Success);
        Assert.AreSame(token, result.Data);
    }

    [Test]
    public void SuccessFulCreatesSuccessfulResult()
    {
        var token = new JObject();
        var result = JLioExecutionResult.SuccessFul(token);
        Assert.IsTrue(result.Success);
        Assert.AreSame(token, result.Data);
    }
}
