using JLio.Client;
using JLio.Core.Contracts;
using JLio.Core.Models;
using JLio.Extensions.Math;
using JLio.Extensions.Text;
using JLio.Extensions.ETL;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace JLio.UnitTests.CommandsTests;

public class ParentNavigationTests
{
    private IExecutionContext executionContext;
    private IParseOptions parseOptions;

    [SetUp]
    public void Setup()
    {
        parseOptions = ParseOptions.CreateDefault().RegisterMath().RegisterText().RegisterETL();
        executionContext = ExecutionContext.CreateDefault();
    }

    [Test]
    public void ParentNavigation_SingleLevel_ShouldWork()
    {
        var testData = JToken.Parse(@"{
            ""departments"": [
                {
                    ""name"": ""Engineering"",
                    ""employees"": [
                        {""name"": ""Alice"", ""role"": ""Developer""}
                    ]
                }
            ]
        }");

        var script = @"[{
            ""path"": ""$.departments[*].employees[*].departmentName"",
            ""value"": ""=fetch(@.<--.name)"",
            ""command"": ""add""
        }]";

        var result = JLioConvert.Parse(script, parseOptions).Execute(testData, executionContext);

        Assert.IsTrue(result.Success);
        Assert.AreEqual("Engineering", result.Data.SelectToken("$.departments[0].employees[0].departmentName")?.Value<string>());
    }

    [Test]
    public void ParentNavigation_MultipleLevel_ShouldWork()
    {
        var testData = JToken.Parse(@"{
            ""company"": {
                ""name"": ""TechCorp"",
                ""departments"": [
                    {
                        ""name"": ""Engineering"",
                        ""employees"": [
                            {""name"": ""Alice"", ""role"": ""Developer""}
                        ]
                    }
                ]
            }
        }");

        var script = @"[{
            ""path"": ""$.company.departments[*].employees[*].companyName"",
            ""value"": ""=fetch(@.<--.<--.name)"",
            ""command"": ""add""
        }]";

        var result = JLioConvert.Parse(script, parseOptions).Execute(testData, executionContext);

        Assert.IsTrue(result.Success);
        Assert.AreEqual("TechCorp", result.Data.SelectToken("$.company.departments[0].employees[0].companyName")?.Value<string>());
    }

    [Test]
    public void PathFunction_ParentNavigation_ShouldWork()
    {
        var testData = JToken.Parse(@"{
            ""orders"": [
                {
                    ""id"": ""ORD-001"",
                    ""items"": [
                        {""productId"": ""P001"", ""quantity"": 2}
                    ]
                }
            ]
        }");

        var script = @"[{
            ""path"": ""$.orders[*].items[*].orderPath"",
            ""value"": ""=path(@.<--)"",
            ""command"": ""add""
        }]";

        var result = JLioConvert.Parse(script, parseOptions).Execute(testData, executionContext);

        Assert.IsTrue(result.Success);
        Assert.AreEqual("$.orders[0]", result.Data.SelectToken("$.orders[0].items[0].orderPath")?.Value<string>());
    }
}