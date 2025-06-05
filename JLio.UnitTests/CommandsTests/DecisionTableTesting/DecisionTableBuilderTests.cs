using System.Collections.Generic;
using JLio.Commands.Builders;
using JLio.Core.Models;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace JLio.UnitTests.CommandsTests.DecisionTableTesting;

public class DecisionTableBuilderTests
{
    [Test]
    public void BuilderAddsCommand()
    {
        var config = new DecisionTableConfig
        {
            Inputs = new List<DecisionInput>
            {
                new DecisionInput { Name = "age", Path = "@.age", Type = "number" }
            },
            Outputs = new List<DecisionOutput>
            {
                new DecisionOutput { Name = "category", Path = "@.category" }
            },
            Rules = new List<DecisionRule>
            {
                new DecisionRule
                {
                    Conditions = new Dictionary<string, JToken> { { "age", ">=18" } },
                    Results = new Dictionary<string, JToken> { { "category", "adult" } }
                }
            }
        };

        var data = JObject.Parse("{ \"person\": { \"age\": 20 } }");
        var script = new JLioScript()
            .DecisionTable("$.person")
            .With(config);

        var result = script.Execute(data);

        Assert.IsTrue(result.Success);
        Assert.AreEqual("adult", result.Data.SelectToken("$.person.category")?.ToString());
    }
}

