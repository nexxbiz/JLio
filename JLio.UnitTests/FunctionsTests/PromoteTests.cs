using JLio.Client;
using JLio.Commands.Builders;
using JLio.Core.Contracts;
using JLio.Core.Models;
using JLio.Functions;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace JLio.UnitTests.FunctionsTests
{
    public class PromoteTests
    {
        private IExecutionContext executeContext;
        private ParseOptions parseOptions;

        [SetUp]
        public void Setup()
        {
            parseOptions = ParseOptions.CreateDefault();
            executeContext = ExecutionContext.CreateDefault();
        }

        [TestCase("=promote($.source,'new')", "{\"source\" : 1}", "{\"new\": 1 }")]
        [TestCase("=promote($.source,'new')", "{\"source\" : [1,2]}", "{\"new\": [1,2] }")]
        [TestCase("=promote($.source,'new')", "{\"source\" : \"1\"}", "{\"new\": \"1\" }")]
        public void ScriptTestWithPath(string function, string data, string expectedResult)
        {
            var script = $"[{{\"path\":\"$.result\",\"value\":\"{function}\",\"command\":\"add\"}}]";
            var result = JLioConvert.Parse(script, parseOptions).Execute(JToken.Parse(data), executeContext);

            Assert.IsTrue(result.Success);
            Assert.IsTrue(executeContext.Logger.LogEntries.TrueForAll(i => i.Level != LogLevel.Error));
            Assert.IsTrue(JToken.DeepEquals(JToken.Parse(expectedResult), result.Data.SelectToken("$.result")));
        }

        [TestCase("=promote('new')", "{\"result\" : [1,2]}", "[ {\"new\": 1},{\"new\": 2}}")]
        public void ScriptTestOnSelfArray(string function, string data, string expectedResult)
        {
            var script = $"[{{\"path\":\"$.result[*]\",\"value\":\"{function}\",\"command\":\"set\"}}]";
            var result = JLioConvert.Parse(script, parseOptions).Execute(JToken.Parse(data), executeContext);

            Assert.IsTrue(result.Success);
            Assert.IsTrue(executeContext.Logger.LogEntries.TrueForAll(i => i.Level != LogLevel.Error));
            Assert.IsTrue(JToken.DeepEquals(JToken.Parse(expectedResult), result.Data.SelectToken("$.result")));
        }

        [TestCase("=promote('new')", "{\"result\" : [1,2]}", "[{\"new\": 1 }, {\"new\": 2 }]")]
        public void ScriptTestOnSelf(string function, string data, string expectedResult)
        {
            var script = $"[{{\"path\":\"$.result[*]\",\"value\":\"{function}\",\"command\":\"set\"}}]";
            var result = JLioConvert.Parse(script, parseOptions).Execute(JToken.Parse(data), executeContext);

            Assert.IsTrue(result.Success);
            Assert.IsTrue(executeContext.Logger.LogEntries.TrueForAll(i => i.Level != LogLevel.Error));
            Assert.IsTrue(JToken.DeepEquals(JToken.Parse(expectedResult), result.Data.SelectToken("$.result")));
        }

        [Test]
        public void CanBeUsedInFluentApi()
        {
            var script = new JLioScript()
                    .Set(new Promote("$.demo", "new"))
                    .OnPath("$.id")
                ;
            var result = script.Execute(JToken.Parse("{\"demo\" : 1}"));

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Success);
        }
    }
}