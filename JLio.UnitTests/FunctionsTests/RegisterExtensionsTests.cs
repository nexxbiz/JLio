using JLio.Core;
using JLio.Extensions.JSchema;
using NUnit.Framework;

namespace JLio.UnitTests.FunctionsTests;

public class RegisterExtensionsTests
{
    [Test]
    public void RegisterJSchemaFunctionsRegistersFilterBySchema()
    {
        var provider = new FunctionsProvider();
        provider.RegisterJSchemaFunctions();
        Assert.IsNotNull(provider["filterBySchema"]);
    }

    [Test]
    public void RegisterFilterByJSchemaFunctionRegistersFilterBySchema()
    {
        var provider = new FunctionsProvider();
        provider.RegisterFilterByJSchemaFunction();
        Assert.IsNotNull(provider["filterBySchema"]);
    }
}
