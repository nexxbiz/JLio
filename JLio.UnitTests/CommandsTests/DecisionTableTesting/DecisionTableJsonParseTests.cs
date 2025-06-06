using JLio.Client;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace JLio.UnitTests.CommandsTests.DecisionTableTesting;

public class DecisionTableJsonParseTests
{
    [Test]
    public void ParseDecisionTableScriptAndExecute()
    {
        var script = "[{\"command\":\"decisionTable\",\"path\":\"$.person\",\"decisionTable\":{\"inputs\":[{\"name\":\"age\",\"path\":\"@.age\"}],\"outputs\":[{\"name\":\"category\",\"path\":\"@.category\"}],\"rules\":[{\"conditions\":{\"age\":\">=18\"},\"results\":{\"category\":\"adult\"}}]}}]";
        var scriptObj = JLioConvert.Parse(script);
        var data = JObject.Parse("{\"person\":{\"age\":20}}");
        var result = scriptObj.Execute(data);
        Assert.IsTrue(result.Success);
        Assert.AreEqual("adult", data.SelectToken("$.person.category")?.ToString());
    }
}
